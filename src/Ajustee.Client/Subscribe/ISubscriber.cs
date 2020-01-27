using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal interface ISubscriber : IDisposable
    {
        ISocketClient Client { get; }
        void Subscribe(string path, IDictionary<string, string> properties);
        Task SubscribeAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken);
        void Unsubscribe(string path);
        Task UnsubscribeAsync(string path, CancellationToken cancellationToken);
    }
}
