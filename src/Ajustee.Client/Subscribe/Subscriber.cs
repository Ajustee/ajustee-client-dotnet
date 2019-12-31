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

        private readonly SemaphoreSlim m_SyncRoot = new SemaphoreSlim(1, 1);
        private CancellationTokenSource m_CancellationTokenSource;
        private bool m_Initialized;

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
            var _cancellationToken = m_CancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
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
                        catch (TaskCanceledException)
                        {
                            throw;
                        }
                        catch (Exception _ex)
                        {
                            Debug.WriteLine($"Occured error: {_ex.Message}");
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Cancelled");
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Cancelled");
                }
                catch (ConnectionClosedException _ex)
                {
                    Debug.WriteLine($"Disconnected ({_ex.ErrorCode})");
                }
                catch (Exception _ex)
                {
                    Debug.WriteLine($"Occured error: {_ex.Message}");
                }
            }, _cancellationToken);
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
                    InitializAndConnect(path, properties).Wait();
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
                SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token).Wait();
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
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
                await SendCommandAsync(new WsSubscribeCommand(Settings, path, properties), m_CancellationTokenSource.Token);
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
