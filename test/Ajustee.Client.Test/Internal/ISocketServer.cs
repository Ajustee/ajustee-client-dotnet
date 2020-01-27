
namespace Ajustee
{
    internal interface ISocketServer
    {
        void Send(byte[] message);
        void Send(int closeStatus);
        void Unavailable(int attempts);
    }
}
