using System.Collections.Generic;
using System.IO;

namespace Ajustee
{
    internal interface IJsonSerializer
    {
        #region Methods region

        string Serialize(object obj);

        T Deserialize<T>(Stream jsonStream);

#if ASYNC
        System.Threading.Tasks.Task<T> DeserializeAsync<T>(Stream jsonStream, System.Threading.CancellationToken cancellationToken = default);
#endif

        #endregion
    }
}
