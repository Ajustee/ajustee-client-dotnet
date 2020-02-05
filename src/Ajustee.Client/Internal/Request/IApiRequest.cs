using System;
using System.Collections.Generic;
using System.IO;

namespace Ajustee
{
    internal interface IApiRequest : IDisposable
    {
        #region Methods region

        Stream GetStream(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties);
        void Update(AjusteeConnectionSettings settings, string path, string value);
#if ASYNC
        System.Threading.Tasks.Task<Stream> GetStreamAsync(AjusteeConnectionSettings settings, string path, IDictionary<string, string> properties, System.Threading.CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task UpdateAsync(AjusteeConnectionSettings settings, string path, string value, System.Threading.CancellationToken cancellationToken = default);
#endif

        #endregion
    }
}
