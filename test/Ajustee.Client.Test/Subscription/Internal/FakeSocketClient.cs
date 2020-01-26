using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeSocketClient : ISocketClient
    {
        private readonly ConcurrentQueue<object> m_ReceiveQueue = new ConcurrentQueue<object>();
        private readonly SemaphoreSlim m_ReceiveQueueWaiter = new SemaphoreSlim(0);
        private readonly CancellationTokenSource m_CancellationSource = new CancellationTokenSource();
        private ArraySegment<byte>? m_Buffering;
        private int m_BufferingOffset = 0;
        private static int? m_UnavailableAttempts;

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            // Invalid uri
            if (uri.Host == "invalid.url")
                throw new WebSocketException(WebSocketError.NotAWebSocket);

            if (m_UnavailableAttempts != null)
            {
                if (--m_UnavailableAttempts < 1) m_UnavailableAttempts = null;
                throw new WebSocketException(WebSocketError.Faulted);
            }

            return Task.FromResult(0);
        }

        public void Dispose()
        {
            m_CancellationSource.Cancel();
            m_ReceiveQueueWaiter.Release();
            m_CancellationSource.Dispose();
        }

        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            var _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationSource.Token, cancellationToken);
            while (true)
            {
                if (m_Buffering == null)
                {
                    await m_ReceiveQueueWaiter.WaitAsync(_cancellationSource.Token);

                    if (m_ReceiveQueue.TryDequeue(out var _queueItem))
                    {
                        if (_queueItem is ArraySegment<byte> _queueBuffer)
                        {
                            if (buffer.Count < _queueBuffer.Count)
                            {
                                Buffer.BlockCopy(_queueBuffer.Array, 0, buffer.Array, 0, buffer.Count);
                                m_Buffering = _queueBuffer;
                                m_BufferingOffset = buffer.Count;

                                return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Text, false);
                            }
                            else
                            {
                                Buffer.BlockCopy(_queueBuffer.Array, 0, buffer.Array, 0, _queueBuffer.Count);

                                return new WebSocketReceiveResult(_queueBuffer.Count, WebSocketMessageType.Text, true);
                            }
                        }
                        else if (_queueItem is WebSocketCloseStatus _closeStatus)
                        {
                            return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, _closeStatus, _closeStatus.ToString());
                        }
                    }
                }
                else
                {
                    var _count = m_Buffering.Value.Count - m_BufferingOffset;
                    if (buffer.Count < _count)
                    {
                        Buffer.BlockCopy(m_Buffering.Value.Array, m_BufferingOffset, buffer.Array, 0, buffer.Count);
                        m_BufferingOffset += buffer.Count;

                        return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Text, false);
                    }
                    else
                    {
                        Buffer.BlockCopy(m_Buffering.Value.Array, m_BufferingOffset, buffer.Array, 0, _count);
                        m_Buffering = null;
                        m_BufferingOffset = 0;

                        return new WebSocketReceiveResult(_count, WebSocketMessageType.Text, true);
                    }
                }
            }
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public void SetRequestHeader(string headerName, string headerValue)
        {
        }

        public void SetReceive(ArraySegment<byte> buffer)
        {
            m_ReceiveQueue.Enqueue(buffer);
            m_ReceiveQueueWaiter.Release();
        }

        public void SetReceive(WebSocketCloseStatus closeStatus)
        {
            m_ReceiveQueue.Enqueue(closeStatus);
            m_ReceiveQueueWaiter.Release();
        }

        public void Unavailable(int attempts)
        {
            m_UnavailableAttempts = attempts;
        }
    }
}
