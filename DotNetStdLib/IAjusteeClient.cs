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

        IEnumerable<ConfigKey> GetConfigurations(IDictionary<string, string> headers);

        IEnumerable<ConfigKey> GetConfigurations(string path, IDictionary<string, string> headers);

#if ASYNC
        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(IDictionary<string, string> headers, CancellationToken cancellationToken = default);

        Task<IEnumerable<ConfigKey>> GetConfigurationsAsync(string path, IDictionary<string, string> headers, CancellationToken cancellationToken = default);
#endif

        #endregion
    }
}
