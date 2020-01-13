using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            m_JsonOptions = new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.IsoDateFormat, NullValueHandling = NullValueHandling.Ignore };
            m_JsonOptions.Converters.Add(new StringEnumConverter());
            m_Serializer = JsonSerializer.Create(m_JsonOptions);
        }

        #endregion

        #region Public methods region

        public string Serialize(object obj)
        {
            var _stringBuilder = new StringBuilder();
            m_Serializer.Serialize(new StringWriter(_stringBuilder), obj);
            return _stringBuilder.ToString();
        }

        public T Deserialize<T>(string json)
        {
            using var _jsonReader = new JsonTextReader(new StringReader(json));
            return m_Serializer.Deserialize<T>(_jsonReader);
        }

        public T Deserialize<T>(Stream jsonStream)
        {
            using var _jsonReader = new JsonTextReader(new StreamReader(jsonStream, Encoding.UTF8));
            return m_Serializer.Deserialize<T>(_jsonReader);
        }

#if ASYNC
        public async System.Threading.Tasks.Task<T> DeserializeAsync<T>(Stream jsonStream, System.Threading.CancellationToken cancellationToken = default)
        {
            using var _jsonReader = new JsonTextReader(new StreamReader(jsonStream, Encoding.UTF8));
            return await System.Threading.Tasks.Task.FromResult(m_Serializer.Deserialize<T>(_jsonReader));
        }
#endif

        #endregion
    }
}
