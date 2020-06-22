using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;
using ReceiveCallbackHandler = System.Action<System.Collections.Generic.IEnumerable<Ajustee.ConfigKey>>;
using DeletedCallbackHandler = System.Action<string>;

namespace Ajustee
{
    internal class Subscriber<TSocketClient> : ISubscriber
        where TSocketClient : ISocketClient, new()
    {
        private class SubscribedItem
        {
            public string Path;
            public IDictionary<string, string> Properties;
            public bool Confirmed;
        }

        private const int m_RECEIVE_STATE_NONE = 0;
        private const int m_RECEIVE_STATE_ACTIVE = 1;
        private const int m_SOCKET_CLOSE_CODE_NORMAL = 1000; // Successful operation / regular socket shutdown.

        private readonly SemaphoreSlim m_SyncRoot = new SemaphoreSlim(1, 1);
        private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
        private bool m_Initialized;
        private List<SubscribedItem> m_Subscribed;
        private int m_ReceiveState = m_RECEIVE_STATE_NONE;
        private const int m_BufferSize = 4096;
        private readonly ReceiveCallbackHandler m_ReceiveCallback;
        private readonly DeletedCallbackHandler m_DeletedCallback;
        private TSocketClient m_Client;

        protected readonly AjusteeConnectionSettings Settings;
        protected internal int ReconnectInitDelay = 30_000; // 30 seconds

        public ISocketClient Client => m_Client;

        private Task InitializAsync()
        {
            // Connects the web socket.
            var _connectTask = ConnectAsync(m_CancellationTokenSource.Token);

            // Registers recieve method in web socket.
            _connectTask.ContinueWith(ReceiveImpl, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.LongRunning);

            // Sets initialized state.
            m_Initialized = true;

            return _connectTask;
        }

        private void ReceiveImpl(Task _)
        {
            if (Interlocked.CompareExchange(ref m_ReceiveState, m_RECEIVE_STATE_ACTIVE, m_RECEIVE_STATE_NONE) != m_RECEIVE_STATE_NONE)
                return;

            var _cancellationToken = m_CancellationTokenSource.Token;

            Task.Run(async () =>
            {
                var _reconnectCounter = 0;
                var _reconnect = false;
                do
                {
                    try
                    {
                        // Try reconnect if requires.
                        if (_reconnect)
                        {
                            // Delay before reconnect.
                            // Delay time will increase till 5 minutes of end.
                            // Used fot this following formula: initial * (log(counter, increase_factor) + 1) => maximum is 5 minutes, initial is 30 seconds.
                            // To skip increasing delay have to set 0 for ReconnectInitDelay.
                            var _delay = (int)(ReconnectInitDelay * (Math.Log(++_reconnectCounter, 10.88632d) + 1));
                            if (_delay < 1) _delay = 1;
                            Debug.WriteLine($"Reconnects after {_delay / 1000} seconds");
                            await Task.Delay(_delay, _cancellationToken);

                            // Reconnect the all subscriptions.
                            Debug.WriteLine("Reconnecting");
                            await Reconnect(_cancellationToken);
                        }

                        while (!_cancellationToken.IsCancellationRequested)
                        {
                            var _memory = new MemoryStream();
                            try
                            {
                                // Received respose from source.
                                if (await ReceiveAsync(_memory, _cancellationToken))
                                {
                                    // Gets received message.
                                    _memory.Seek(0, SeekOrigin.Begin);
                                    var _message = JsonSerializer.Deserialize<ReceiveMessage>(_memory);

                                    // Invokes implementation of after receive message.
                                    OnReceiveMessage(_message);
                                }
                            }
                            catch (ConnectionClosedException _ex)
                            {
                                ATL.WriteLine($"Subscriber closed({_ex.ErrorCode})");
                                throw;
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception _ex)
                            {
                                Debug.WriteLine($"Occured error: {_ex.Message}");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Cancelled");
                        _reconnect = false;
                    }
                    catch (ConnectionClosedException _ex)
                    {
                        Debug.WriteLine($"Disconnected ({_ex.ErrorCode})");
                        _reconnect = _ex.Reconnect && Settings.ReconnectSubscriptions;

                        if (_ex.ErrorCode == m_SOCKET_CLOSE_CODE_NORMAL && !HasAnySubscriptions())
                            _reconnect = false;
                    }
                    catch (Exception _ex)
                    {
                        Debug.WriteLine($"Occured error: {_ex.Message}");
                        _reconnect = false;
                    }
                }
                while (_reconnect && !_cancellationToken.IsCancellationRequested);

                // Resets receive state.
                Interlocked.Exchange(ref m_ReceiveState, m_RECEIVE_STATE_NONE);

                // Give to reconnect next time.
                m_Initialized = false;

            }, _cancellationToken);
        }

        private int SetBeingSubscribed(string path, IDictionary<string, string> properties)
        {
            if (m_Subscribed == null)
                m_Subscribed = new List<SubscribedItem>();
            m_Subscribed.Add(new SubscribedItem { Path = path, Properties = properties, Confirmed = false });
            return m_Subscribed.Count - 1;
        }

        private void ConfirmSubscribed(string path)
        {
            if (m_Subscribed == null) return;
            for (int i = m_Subscribed.Count - 1; i >= 0; i--)
            {
                if (m_Subscribed[i].Path == path)
                    m_Subscribed[i].Confirmed = true;
            }
        }

        private void RemoveSubscribed(string path)
        {
            if (m_Subscribed == null) return;
            for (int i = m_Subscribed.Count - 1; i >= 0; i--)
            {
                if (m_Subscribed[i].Path == path)
                    m_Subscribed.RemoveAt(i);
            }
        }

        private void RemoveSubscribed(int index)
        {
            if (m_Subscribed == null) return;
            m_Subscribed.RemoveAt(index);
        }

        private bool HasAnySubscriptions()
        {
            if (m_Subscribed == null) return false;
            return m_Subscribed.Count != 0;
        }

        public void SubscribeInternal(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            // Validate properties.
            ValidateProperties(Settings.DefaultProperties);
            ValidateProperties(properties);

            // Get merged properties.
            properties = GetMergedProperties(Settings.TrackerId == null ? null : new Dictionary<string, string> { { TrackerIdName, FormatPropertyValue(Settings.TrackerId) } },
                Settings.DefaultProperties, properties);

            if (!m_Initialized)
                InitializAsync().GetAwaiter().GetResult();

            // Sents subscribe command for next subscriptions.
            var _beingIndex = SetBeingSubscribed(path, properties);
            try
            {
                SendCommandAsync(new WsSubscribeCommand(path, properties), cancellationToken).GetAwaiter().GetResult();
            }
            catch
            {
                RemoveSubscribed(_beingIndex);
            }
        }

        public async Task SubscribeAsyncInternal(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            // Validate properties.
            ValidateProperties(Settings.DefaultProperties);
            ValidateProperties(properties);

            // Get merged properties.
            properties = GetMergedProperties(Settings.TrackerId == null ? null : new Dictionary<string, string> { { TrackerIdName, FormatPropertyValue(Settings.TrackerId) } },
                Settings.DefaultProperties, properties);

            if (!m_Initialized)
                await InitializAsync();

            // Sents subscribe command for next subscriptions.
            var _beingIndex = SetBeingSubscribed(path, properties);
            try
            {
                await SendCommandAsync(new WsSubscribeCommand(path, properties), cancellationToken);
            }
            catch
            {
                RemoveSubscribed(_beingIndex);
            }
        }

        public void UnsubscribeInternal(string path, CancellationToken cancellationToken)
        {
            if (!m_Initialized)
                InitializAsync().GetAwaiter().GetResult();

            // Sents subscribe command for next subscriptions.
            SendCommandAsync(new WsUnsubscribeCommand(path), cancellationToken).GetAwaiter().GetResult();
        }

        public async Task UnsubscribeAsyncInternal(string path, CancellationToken cancellationToken)
        {
            if (!m_Initialized)
                await InitializAsync();

            // Sents subscribe command for next subscriptions.
            await SendCommandAsync(new WsUnsubscribeCommand(path), cancellationToken);
        }

        private async Task Reconnect(CancellationToken cancellationToken)
        {
            ATL.WriteLine("Subscriber reconnecting");

            await m_SyncRoot.WaitAsync(m_CancellationTokenSource.Token);
            var _subscribed = m_Subscribed;
            var _initialized = m_Initialized;
            try
            {
                // Reset internals before reconnect.
                m_Initialized = false;
                m_Subscribed = null;

                foreach (var _item in _subscribed)
                {
                    if (_item.Confirmed)
                        await SubscribeAsyncInternal(_item.Path, _item.Properties, cancellationToken);
                }

                ATL.WriteLine("Subscriber reconnected");
            }
            catch (Exception _ex)
            {
                ATL.WriteLine("Subscriber reconnect failed");

                // Restore internals.
                m_Initialized = _initialized;
                m_Subscribed = _subscribed;

                // Throw connection closed exception to attemp reconnect again.
                throw new ConnectionClosedException(true, (_ex as WebSocketException)?.ErrorCode ?? 0, _ex);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public Subscriber(AjusteeConnectionSettings settings, ReceiveCallbackHandler receiveCallback, DeletedCallbackHandler deletedCallback)
            : base()
        {
            Settings = settings;
            m_ReceiveCallback = receiveCallback;
            m_DeletedCallback = deletedCallback;
        }

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            m_SyncRoot.Wait(m_CancellationTokenSource.Token);
            try
            {
                SubscribeInternal(path, properties, m_CancellationTokenSource.Token);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public async Task SubscribeAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            using var _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token, cancellationToken);

            await m_SyncRoot.WaitAsync(_cancellationSource.Token);
            try
            {
                await SubscribeAsyncInternal(path, properties, _cancellationSource.Token);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public void Unsubscribe(string path)
        {
            m_SyncRoot.Wait(m_CancellationTokenSource.Token);
            try
            {
                UnsubscribeInternal(path, m_CancellationTokenSource.Token);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public async Task UnsubscribeAsync(string path, CancellationToken cancellationToken)
        {
            using var _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token, cancellationToken);

            await m_SyncRoot.WaitAsync(_cancellationSource.Token);
            try
            {
                await UnsubscribeAsyncInternal(path, _cancellationSource.Token);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public void Dispose()
        {
            if (m_CancellationTokenSource != null)
                m_CancellationTokenSource.Cancel();

            lock (this)
            {
                if (m_Client != null)
                {
                    m_Client.Dispose();
                    m_Client = default;
                }
            }
        }

        protected async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // Creates socket client.
            var _client = new TSocketClient();
            _client.SetRequestHeader(AppIdName, Settings.ApplicationId);

            // Connects the web socket.
            await _client.ConnectAsync(GetSubscribeUrl(Settings.ApiUrl), cancellationToken).ConfigureAwait(true);

            // Try to dispose previous websocket.
            if (m_Client != null) m_Client.Dispose();
            m_Client = _client;

            ATL.WriteLine("Subscriber connected");
        }

        protected async Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            await m_Client.SendAsync(command.GetBinary(), WebSocketMessageType.Text, true, cancellationToken);

            ATL.WriteLine($"Subscriber send: {command}");
        }

        protected async Task<bool> ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            var _buffer = new ArraySegment<byte>(new byte[m_BufferSize]);
            WebSocketReceiveResult _result;
            var _hasBytes = false;
            do
            {
                _result = await m_Client.ReceiveAsync(_buffer, cancellationToken);
                if (_result == null) return false;

                // Check to close result.
                if (_result.MessageType == WebSocketMessageType.Close)
                    throw new ConnectionClosedException(true, (int)_result.CloseStatus);

                // Appends to the received data to the memory.
                stream.Write(_buffer.Array, 0, _result.Count);

                _hasBytes |= _result.Count > 0;
            }
            while (!_result.EndOfMessage);
            return _hasBytes;
        }

        protected void OnReceiveMessage(ReceiveMessage message)
        {
            ATL.WriteLine($"Subscriber received: {message}");

            switch (message.Type)
            {
                case ReceiveMessage.SubscribeType:
                    {
                        if (message.Data is SubscriptionMessageData _data && _data.StatusCode == ReceiveMessageStatusCode.Success)
                            ConfirmSubscribed(_data.Path);
                        break;
                    }

                case ReceiveMessage.UnsubscribeType:
                    {
                        if (message.Data is SubscriptionMessageData _data && _data.StatusCode == ReceiveMessageStatusCode.Success)
                            RemoveSubscribed(_data.Path);
                        break;
                    }

                case ReceiveMessage.ChangedType:
                    {
                        if (message.Data is IEnumerable<ConfigKey> _configKeys)
                        {
                            // Raises receive callback.
                            m_ReceiveCallback(_configKeys);
                        }
                        break;
                    }

                case ReceiveMessage.DeletedType:
                    {
                        if (message.Data is string _path)
                        {
                            RemoveSubscribed(_path);

                            // Raises deleted callback.
                            m_DeletedCallback(_path);
                        }
                        break;
                    }
            }
        }
    }
}
