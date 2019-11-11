using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Ajustee
{
    internal class NewtonsoftJsonSerializer : IJsonSerializer
    {
        #region Private fields region

        private readonly JsonSerializerSettings m_JsonOptions;
        private readonly JsonSerializer m_Serializer;

        #endregion

        #region Public constructors region

        public NewtonsoftJsonSerializer()
            : base()
        {
            m_JsonOptions = new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.IsoDateFormat };
            m_Serializer = JsonSerializer.Create(m_JsonOptions);
        }

        #endregion

        #region Public methods region

        public IEnumerable<ConfigKey> Deserialize(Stream jsonStream)
        {
            using var _jsonReader = new JsonTextReader(new StreamReader(jsonStream, Encoding.UTF8));
            return m_Serializer.Deserialize<IEnumerable<ConfigKey>>(_jsonReader);
        }

#if ASYNC
        public async System.Threading.Tasks.Task<IEnumerable<ConfigKey>> DeserializeAsync(Stream jsonStream, System.Threading.CancellationToken cancellationToken = default)
        {
            using var _jsonReader = new JsonTextReader(new StreamReader(jsonStream, Encoding.UTF8));
            return await System.Threading.Tasks.Task.FromResult(m_Serializer.Deserialize<IEnumerable<ConfigKey>>(_jsonReader));
        }
#endif

        #endregion
    }
}
