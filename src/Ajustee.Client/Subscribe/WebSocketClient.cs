using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class WebSocketClient : IWebSocketClient
    {
        private readonly ClientWebSocket m_Client = new ClientWebSocket();
        public void SetRequestHeader(string headerName, string headerValue) => m_Client.Options.SetRequestHeader(headerName, headerValue);
        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) => m_Client.ConnectAsync(uri, cancellationToken);
        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) => m_Client.ReceiveAsync(buffer, cancellationToken);
        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) => m_Client.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        public void Dispose() => m_Client.Dispose();
    }
}
