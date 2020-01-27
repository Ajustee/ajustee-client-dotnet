using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeSocketServer : ISocketServer
    {
        private readonly IAjusteeClient m_Client;

        public FakeSocketServer(IAjusteeClient client)
            : base()
        {
            m_Client = client;
        }

        private FakeSocketClient GetClient()
        {
            return (m_Client as AjusteeClient)?.Subscriber?.Client is FakeSocketClient _client ? _client : null;
        }

        public void Send(byte[] data)
        {
            GetClient()?.SetReceive(new ArraySegment<byte>(data));
        }

        public void Send(int closeStatus)
        {
            GetClient()?.SetReceive((WebSocketCloseStatus)closeStatus);
        }

        public void Unavailable(int attempts)
        {
            GetClient()?.Unavailable(attempts);
        }
    }
}
