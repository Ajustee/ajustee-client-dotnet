using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeSocketClient : IWebSocketClient
    {
        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return Task.FromResult<WebSocketReceiveResult>(null);
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public void SetRequestHeader(string headerName, string headerValue)
        {
        }
    }
}
