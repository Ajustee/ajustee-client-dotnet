using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal abstract class Subscriber : IDisposable
    {
        #region Private fields region

        private const int m_ReconnectDelay = 30_000; // 30 seconds
        private const int m_RECEIVE_STATE_NONE = 0;
        private const int m_RECEIVE_STATE_ACTIVE = 0;

        private readonly SemaphoreSlim m_SyncRoot = new SemaphoreSlim(1, 1);
        private CancellationTokenSource m_CancellationTokenSource;
        private bool m_Initialized;
        private List<KeyValuePair<string, IDictionary<string, string>>> m_Subscribed;
        private int m_ReceiveState = m_RECEIVE_STATE_NONE;

        #endregion

        #region Private methods region

        private Task InitializAndConnect(string path, IDictionary<string, string> properties)
        {
            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(Settings.DefaultProperties);

            // Initializes cancellation token source.
            m_CancellationTokenSource = new CancellationTokenSource();

            // Connects the web socket.
            var _connectTask = ConnectAsync(path, properties, m_CancellationTokenSource.Token);

            // Registers recieve method in web socket.
            var _awaiter = _connectTask.ConfigureAwait(false).GetAwaiter();
            _awaiter.OnCompleted(ReceiveImpl);

            // Sets initialized state.
            m_Initialized = true;

            return _connectTask;
        }

        private void ReceiveImpl()
        {
            if (Interlocked.CompareExchange(ref m_ReceiveState, m_RECEIVE_STATE_ACTIVE, m_RECEIVE_STATE_NONE) != m_RECEIVE_STATE_NONE)
                return;

            var _cancellationToken = m_CancellationTokenSource.Token;

            Task.Run(async () =>
            {
                var _reconnect = false;
                do
                {
                    try
                    {
                        // try reconnect if requires.
                        if (_reconnect)
                        {
                            // Delay before reconnect.
                            await Task.Delay(m_ReconnectDelay, _cancellationToken);

                            // Reconnect the all subscriptions.
                            await Reconnect(_cancellationToken);
                        }

                        while (!_cancellationToken.IsCancellationRequested)
                        {
                            var _memory = new MemoryStream();
                            try
                            {
                                // Received respose from source.
                                await ReceiveAsync(_memory, _cancellationToken);

                                // Gets received message.
                                _memory.Seek(0, SeekOrigin.Begin);
                                var _message = JsonSerializer.Deserialize<ReceiveMessage>(_memory);

                                // Invokes implementation of after receive message.
                                OnReceiveMessage(_message);
                            }
                            catch (ConnectionClosedException)
                            {
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
                        _reconnect = _ex.Reconnect;
                    }
                    catch (Exception _ex)
                    {
                        Debug.WriteLine($"Occured error: {_ex.Message}");
                        _reconnect = false;
                    }
                }
                while (_reconnect);
            }, _cancellationToken);
        }

        private void SetSubscribed(string path, IDictionary<String, string> properties)
        {
            if (m_Subscribed == null)
                m_Subscribed = new List<KeyValuePair<string, IDictionary<string, string>>>();
            m_Subscribed.Add(new KeyValuePair<string, IDictionary<string, string>>(path, properties));
        }

        private async Task Reconnect(CancellationToken cancellationToken)
        {
            m_SyncRoot.Wait();
            var _subscribed = m_Subscribed;
            try
            {
                // Reset internals before reconnect.
                m_Subscribed = null;

                foreach (var _item in _subscribed)
                    await SubscribeAsync(_item.Key, _item.Value);
            }
            catch
            {
                // Restore internals.
                m_Subscribed = _subscribed;
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        #endregion

        #region Public constructors region

        public Subscriber(AjusteeConnectionSettings settings)
            : base()
        {
            Settings = settings;
        }

        #endregion

        #region Public methods region

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            var _initalConnected = false;
            m_SyncRoot.Wait();
            try
            {
                if (!m_Initialized)
                {
                    InitializAndConnect(path, properties).GetAwaiter().GetResult();
                    SetSubscribed(path, properties);
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
            {
                SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token).GetAwaiter().GetResult();
                SetSubscribed(path, properties);
            }
        }

        public async Task SubscribeAsync(string path, IDictionary<string, string> properties)
        {
            var _initalConnected = false;
            await m_SyncRoot.WaitAsync();
            try
            {
                if (!m_Initialized)
                {
                    await InitializAndConnect(path, properties);
                    SetSubscribed(path, properties);
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
            {
                await SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token);
                SetSubscribed(path, properties);
            }
        }

        public virtual void Dispose()
        { }

        #endregion

        #region Protected fields region

        protected readonly AjusteeConnectionSettings Settings;

        #endregion

        #region Protected methods region

        protected abstract Task ConnectAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken);

        protected abstract Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken);

        protected abstract Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken);

        protected abstract void OnReceiveMessage(ReceiveMessage message);

        #endregion
    }
}
