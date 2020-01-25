using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajustee
{
    internal interface ISubscriber : IDisposable
    {
        void Subscribe(string path, IDictionary<string, string> properties);
        Task SubscribeAsync(string path, IDictionary<string, string> properties);
    }
}
