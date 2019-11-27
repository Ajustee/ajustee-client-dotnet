using System.IO;
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

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, obj.GetType(), m_JsonOptions);
        }

        public T Deserialize<T>(Stream jsonStream)
        {
            using var _jsonReader = new StreamReader(jsonStream, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(_jsonReader.ReadToEnd(), m_JsonOptions);
        }

        public async Task<T> DeserializeAsync<T>(Stream jsonStream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(jsonStream, m_JsonOptions, cancellationToken: cancellationToken);
        }

        #endregion
    }
}
