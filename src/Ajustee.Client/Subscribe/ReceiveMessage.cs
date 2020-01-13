using System;
using System.Collections.Generic;

#if NJSON
using Newtonsoft.Json;
#endif

#if SJSON
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace Ajustee
{
    [JsonConverter(typeof(JsonReceiveMessageConverter))]
    internal struct ReceiveMessage
    {
        #region Public fields region

        public const string ConfigKeys = "configkeys";
        public const string Info = "info";
        public const string Reset = "reset";

        #endregion

        #region Public properties region

        public string Type { get; set; }

        public object Data { get; set; }

        #endregion
    }

    internal class JsonReceiveMessageConverter : JsonConverter<ReceiveMessage>
    {
#if SJSON
        public override ReceiveMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string _property = null;
            string _type = null;
            object _data = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        _property = reader.GetString().ToLowerInvariant();
                        break;

                    case JsonTokenType.String:
                    case JsonTokenType.StartArray:
                        if (_property != null)
                        {
                            switch (_property)
                            {
                                case "type":
                                    _type = reader.GetString().ToLowerInvariant();
                                    break;

                                case "data":
                                    switch (_type)
                                    {
                                        case ReceiveMessage.ConfigKeys:
                                            _data = JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(ref reader, options);
                                            _property = null;
                                            break;

                                        case ReceiveMessage.Info:
                                            _data = reader.GetString(); // ConnectionId
                                            _property = null;
                                            break;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            return new ReceiveMessage { Type = _type, Data = _data };
        }

        public override void Write(Utf8JsonWriter writer, ReceiveMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", value.Type);
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, value.Data, options: options);
            writer.WriteEndObject();
        }
#endif

#if NJSON
        public override ReceiveMessage ReadJson(JsonReader reader, Type objectType, ReceiveMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string _type = null;
            object _data = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (reader.Path.ToLowerInvariant())
                        {
                            case "type":
                                _type = reader.ReadAsString().ToLowerInvariant();
                                break;

                            case "data":
                                switch (_type)
                                {
                                    case ReceiveMessage.ConfigKeys:
                                        if (reader.Read())
                                            _data = serializer.Deserialize<IEnumerable<ConfigKey>>(reader);
                                        break;

                                    case ReceiveMessage.Info:
                                        _data = reader.ReadAsString();
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }

            return new ReceiveMessage { Type = _type, Data = _data };
        }

        public override void WriteJson(JsonWriter writer, ReceiveMessage value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type);
            writer.WritePropertyName("data");
            serializer.Serialize(writer, value.Data);
            writer.WriteEndObject();
        }
#endif
    }
}
