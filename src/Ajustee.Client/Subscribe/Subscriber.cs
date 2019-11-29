using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;
using ReceiveCallbackHandler = System.Action<System.Collections.Generic.IEnumerable<Ajustee.ConfigKey>>;

namespace Ajustee
{
    internal class Subscriber : IDisposable
    {
        #region Private fields region

        private const int m_BufferSize = 4096;

        private readonly AjusteeConnectionSettings m_Settings;
        private readonly SemaphoreSlim m_SyncRoot = new SemaphoreSlim(1, 1);
        private readonly ReceiveCallbackHandler m_ReceiveCallback;
        private ClientWebSocket m_WebSocket;
        private CancellationTokenSource m_CancellationTokenSource;

        #endregion

        #region Private methods region

        private Task InitializeWebSocketAndConnect(string path, IDictionary<string, string> properties)
        {
            // Validate properties.
            ValidateProperties(properties);
            ValidateProperties(m_Settings.DefaultProperties);

            // Initializes cancellation token source.
            m_CancellationTokenSource = new CancellationTokenSource();

            // Creates web socket.
            m_WebSocket = new ClientWebSocket();
            m_WebSocket.Options.SetRequestHeader(AppIdName, m_Settings.ApplicationId);
            m_WebSocket.Options.SetRequestHeader(KeyPathName, path);
            if (properties != null)
                m_WebSocket.Options.SetRequestHeader(KeyPropsName, JsonSerializer.Serialize(properties));

            // Connects the web socket.
            var _connectTask = m_WebSocket.ConnectAsync(GetSubscribeUrl(m_Settings.ApiUrl), m_CancellationTokenSource.Token);

            // Registers recieve method in web socket.
            var _awaiter = _connectTask.ConfigureAwait(false).GetAwaiter();
            _awaiter.OnCompleted(RecieveImpl);

            return _connectTask;
        }

        private void RecieveImpl()
        {
            var _cancellationToken = m_CancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        MemoryStream _memory = null;

                        var _buffer = new ArraySegment<byte>(new byte[m_BufferSize]);
                        WebSocketReceiveResult _result = null;
                        do
                        {
                            _result = await m_WebSocket.ReceiveAsync(_buffer, _cancellationToken);

                            // Check to close result.
                            if (_result.MessageType == WebSocketMessageType.Close)
                                throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely);

                            // Appends to the received data to the memory.
                            if (_memory == null)
                                _memory = new MemoryStream(_buffer.Array, 0, _result.Count);
                            else
                                _memory.Write(_buffer.Array, 0, _result.Count);
                        }
                        while (!_result.EndOfMessage);

                        try
                        {
                            // Gets received message.
                            _memory.Seek(0, SeekOrigin.Begin);
                            var _message = JsonSerializer.Deserialize<ReceiveMessage>(_memory);

                            switch (_message.Action)
                            {
                                case ReceiveMessage.ConfigKeys:
                                    {
                                        if (_message.Data is IEnumerable<ConfigKey> _configKeys)
                                        {
                                            // Raises receive callback.
                                            m_ReceiveCallback(_configKeys);
                                        }
                                        break;
                                    }

                                case ReceiveMessage.Info:
                                    {
                                        // Gets connection informations.
                                        throw new NotImplementedException();
                                    }

                                case ReceiveMessage.Reset:
                                    {
                                        // Required reset all and reconnect.
                                        throw new NotImplementedException();
                                    }
                            }
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
                catch (WebSocketException _ex) when (_ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    Debug.WriteLine($"Disconnected ({_ex.WebSocketErrorCode})");
                }
                catch (Exception _ex)
                {
                    Debug.WriteLine($"Occured error: {_ex.Message}");
                }
            }, _cancellationToken);
        }

        private Task SendCommand(WsCommand command)
        {
            return m_WebSocket.SendAsync(command.GetBinary(), WebSocketMessageType.Text, true, m_CancellationTokenSource.Token);
        }

        #endregion

        #region Public constructors region

        public Subscriber(AjusteeConnectionSettings settings, ReceiveCallbackHandler receiveCallback)
            : base()
        {
            m_Settings = settings;
            m_ReceiveCallback = receiveCallback;
        }

        #endregion

        #region Public methods region

        public void Subscribe(string path, IDictionary<string, string> properties)
        {
            var _initalConnected = false;
            m_SyncRoot.Wait();
            try
            {
                if (m_WebSocket == null)
                {
                    InitializeWebSocketAndConnect(path, properties).Wait();
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
                SendCommand(new WsSubscribeCommand(m_Settings, path, properties)).Wait();
        }

        public async Task SubscribeAsync(string path, IDictionary<string, string> properties)
        {
            var _initalConnected = false;
            await m_SyncRoot.WaitAsync();
            try
            {
                if (m_WebSocket == null)
                {
                    await InitializeWebSocketAndConnect(path, properties);
                    _initalConnected = true;
                }
            }
            finally
            {
                m_SyncRoot.Release();
            }

            // Sents subscribe command for next subscriptions.
            if (!_initalConnected)
                await SendCommand(new WsSubscribeCommand(m_Settings, path, properties));
        }

        public void Dispose()
        {
            lock (this)
            {
                if (m_WebSocket != null)
                {
                    m_WebSocket.Dispose();
                    m_WebSocket = null;
                }
            }
        }

        #endregion
    }
}
