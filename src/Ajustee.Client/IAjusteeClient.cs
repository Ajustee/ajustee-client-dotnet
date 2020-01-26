using System;
using System.Collections.Generic;

namespace Ajustee
{
    public interface IAjusteeClient : IDisposable
    {
        #region Methods region

        IEnumerable<ConfigKey> GetConfigurations();

        IEnumerable<ConfigKey> GetConfigurations(string path);

        IEnumerable<ConfigKey> GetConfigurations(IDictionary<string, string> properties);

        IEnumerable<ConfigKey> GetConfigurations(string path, IDictionary<string, string> properties);

#if ASYNC
        System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default);
#endif

#if SUBSCRIBE
        void Subscribe(string path);

        void Subscribe(string path, IDictionary<string, string> properties);

        System.Threading.Tasks.Task SubscribeAsync(string path, System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task SubscribeAsync(string path, IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default);

        void Unsubscribe(string path);

        System.Threading.Tasks.Task UnsubscribeAsync(string path, System.Threading.CancellationToken cancellationToken = default);
#endif

        #endregion
    }
}
