using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            m_JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true };
            m_JsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        #endregion

        #region Public methods region

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, obj.GetType(), m_JsonOptions);
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, m_JsonOptions);
        }

        public T Deserialize<T>(Stream jsonStream)
        {
            using var _jsonReader = new StreamReader(jsonStream, Helper.MessageEncoding);
            return JsonSerializer.Deserialize<T>(_jsonReader.ReadToEnd(), m_JsonOptions);
        }

        public async Task<T> DeserializeAsync<T>(Stream jsonStream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(jsonStream, m_JsonOptions, cancellationToken: cancellationToken);
        }

        #endregion
    }

    internal class JsonConfigValueConverter : JsonConverter<string>
    {
        #region Public methods region

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => "true",
                JsonTokenType.False => "false",
                _ => reader.GetString(),
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        #endregion
    }
}
