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

        private const int m_RECEIVE_STATE_NONE = 0;
        private const int m_RECEIVE_STATE_ACTIVE = 1;

        private readonly SemaphoreSlim m_SyncRoot = new SemaphoreSlim(1, 1);
        private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
        private bool m_Initialized;
        private List<KeyValuePair<string, IDictionary<string, string>>> m_Subscribed;
        private int m_ReceiveState = m_RECEIVE_STATE_NONE;

        #endregion

        #region Private methods region

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
                        if (_reconnect && Settings.ReconnectSubscriptions)
                        {
                            // Delay before reconnect.
                            // Delay time will increase till 5 minutes of end.
                            // Used fot this following formula: initial * (log(counter, increase_factor) + 1) => maximum is 5 minutes, initial is 30 seconds.
                            var _delay = (int)(ReconnectInitDelay * (Math.Log(++_reconnectCounter, 10.88632d) + 1));
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
                while (_reconnect && !_cancellationToken.IsCancellationRequested);
            }, _cancellationToken);
        }

        private void SetSubscribed(string path, IDictionary<String, string> properties)
        {
            if (m_Subscribed == null)
                m_Subscribed = new List<KeyValuePair<string, IDictionary<string, string>>>();
            m_Subscribed.Add(new KeyValuePair<string, IDictionary<string, string>>(path, properties));
        }

        public void SubscribeInternal(string path, IDictionary<string, string> properties)
        {
            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(Settings.DefaultProperties);

            if (!m_Initialized)
                InitializAsync().GetAwaiter().GetResult();

            // Sents subscribe command for next subscriptions.
            SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token).GetAwaiter().GetResult();
            SetSubscribed(path, properties);
        }

        public async Task SubscribeAsyncInternal(string path, IDictionary<string, string> properties)
        {
            if (!m_Initialized)
                await InitializAsync();

            // Sents subscribe command for next subscriptions.
            await SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token);
            SetSubscribed(path, properties);
        }

        private async Task Reconnect(CancellationToken cancellationToken)
        {
            await m_SyncRoot.WaitAsync(m_CancellationTokenSource.Token);
            var _subscribed = m_Subscribed;
            var _initialized = m_Initialized;
            try
            {
                // Reset internals before reconnect.
                m_Initialized = false;
                m_Subscribed = null;

                foreach (var _item in _subscribed)
                    await SubscribeAsyncInternal(_item.Key, _item.Value);
            }
            catch (Exception _ex)
            {
                // Restore internals.
                m_Initialized = _initialized;
                m_Subscribed = _subscribed;

                // Throw connection closed exception to attemp reconnect again.
                throw new ConnectionClosedException(true, _ex);
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
            m_SyncRoot.Wait(m_CancellationTokenSource.Token);
            try
            {
                SubscribeInternal(path, properties);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public async Task SubscribeAsync(string path, IDictionary<string, string> properties)
        {
            await m_SyncRoot.WaitAsync(m_CancellationTokenSource.Token);
            try
            {
                await SubscribeAsyncInternal(path, properties);
            }
            finally
            {
                m_SyncRoot.Release();
            }
        }

        public virtual void Dispose()
        {
            if (m_CancellationTokenSource != null)
                m_CancellationTokenSource.Cancel();
        }

        #endregion

        #region Protected fields region

        protected readonly AjusteeConnectionSettings Settings;
        protected int ReconnectInitDelay = 30_000; // 30 seconds

        #endregion

        #region Protected methods region

        protected abstract Task ConnectAsync(CancellationToken cancellationToken);

        protected abstract Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken);

        protected abstract Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken);

        protected abstract void OnReceiveMessage(ReceiveMessage message);

        #endregion
    }
}
