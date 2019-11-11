using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace Ajustee
{
    internal class SystemJsonSerializer : IJsonSerializer
    {
        #region Private fields region

        private readonly JsonSerializerOptions m_JsonOptions;

        #endregion

        #region Public constructors region

        public SystemJsonSerializer()
            : base()
        {
            m_JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            m_JsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        #endregion

        #region Public methods region

        public IEnumerable<ConfigKey> Deserialize(Stream jsonStream)
        {
            using var _jsonReader = new StreamReader(jsonStream, Encoding.UTF8);
            return JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(_jsonReader.ReadToEnd(), m_JsonOptions);
        }

        public async Task<IEnumerable<ConfigKey>> DeserializeAsync(Stream jsonStream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<ConfigKey>>(jsonStream, m_JsonOptions, cancellationToken: cancellationToken);
        }

        #endregion
    }
}
