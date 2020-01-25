using System.Threading.Tasks;

namespace Ajustee
{
    internal interface ISocketServer
    {
        Task Start();
        Task Stop();
        Task Connect();
        Task Send(byte[] data);
        Task<byte[]> Receive();
    }
}
