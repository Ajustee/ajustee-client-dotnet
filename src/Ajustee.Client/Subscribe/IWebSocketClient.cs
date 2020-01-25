using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal interface IWebSocketClient : IDisposable
    {
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        void SetRequestHeader(string headerName, string headerValue);
    }
}
