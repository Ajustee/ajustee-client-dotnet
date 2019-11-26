using System;
using System.Collections.Generic;
#if ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

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
        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(IDictionary<string, string> properties, CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken = default);
#endif

#if SUBSCRIBE
        void Subscribe(string path);

        void Subscribe(string path, IDictionary<string, string> properties);
#endif

        #endregion
    }
}
