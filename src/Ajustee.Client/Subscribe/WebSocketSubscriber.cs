using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;
using ReceiveCallbackHandler = System.Action<System.Collections.Generic.IEnumerable<Ajustee.ConfigKey>>;

namespace Ajustee
{
    internal class WebSocketSubscriber : Subscriber
    {
        #region Private fields region

        private const int m_BufferSize = 4096;

        private readonly ReceiveCallbackHandler m_ReceiveCallback;
        private ClientWebSocket m_WebSocket;
        private string m_ConnectionId;

        #endregion

        #region Public constructors region

        public WebSocketSubscriber(AjusteeConnectionSettings settings, ReceiveCallbackHandler receiveCallback)
            : base(settings)
        {
            m_ReceiveCallback = receiveCallback;
        }

        #endregion

        #region Public methods region

        public override void Dispose()
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

        #region Protected methods region

        protected override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // Creates web socket.
            var _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader(AppIdName, Settings.ApplicationId);

            // Connects the web socket.
            await _webSocket.ConnectAsync(GetSubscribeUrl(Settings.ApiUrl), cancellationToken).ConfigureAwait(true);

            // Try to dispose previous websocket.
            if (m_WebSocket != null) _webSocket.Dispose();

            m_WebSocket = _webSocket;
        }

        protected override Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            return m_WebSocket.SendAsync(command.GetBinary(), WebSocketMessageType.Text, true, cancellationToken);
        }

        protected override async Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            var _buffer = new ArraySegment<byte>(new byte[m_BufferSize]);
            WebSocketReceiveResult _result = null;
            do
            {
                _result = await m_WebSocket.ReceiveAsync(_buffer, cancellationToken);

                // Check to close result.
                if (_result.MessageType == WebSocketMessageType.Close)
                    throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely, _result.CloseStatusDescription);

                // Appends to the received data to the memory.
                stream.Write(_buffer.Array, 0, _result.Count);
            }
            while (!_result.EndOfMessage);
        }

        protected override void OnReceiveMessage(ReceiveMessage message)
        {
            switch (message.Type)
            {
                case ReceiveMessage.ConfigKeys:
                    {
                        if (message.Data is IEnumerable<ConfigKey> _configKeys)
                        {
                            // Raises receive callback.
                            m_ReceiveCallback(_configKeys);
                        }
                        break;
                    }

                case ReceiveMessage.Info:
                    {
                        if (message.Data is string _connectionId)
                        {
                            // Sets connection id.
                            m_ConnectionId = _connectionId;
                        }
                        break;
                    }

                case ReceiveMessage.Reset:
                    {
                        // Required reset all and reconnect.
                        throw new ConnectionClosedException(true);
                    }
            }
        }

        #endregion
    }
}
