using System.Collections.Generic;
using System.IO;

namespace Ajustee
{
    internal interface IJsonSerializer
    {
        #region Methods region

        IEnumerable<ConfigKey> Deserialize(Stream jsonStream);

#if ASYNC
        System.Threading.Tasks.Task<IEnumerable<ConfigKey>> DeserializeAsync(Stream jsonStream, System.Threading.CancellationToken cancellationToken = default);
#endif

        #endregion
    }
}
