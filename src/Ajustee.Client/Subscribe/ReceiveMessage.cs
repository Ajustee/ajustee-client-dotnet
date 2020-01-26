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
        public const string ChangedType = "changed";
        public const string SubscribeType = "subscribe";
        public const string UnsubscribeType = "unsubscribe";
        public const string DeletedType = "deleted";

        public string Type { get; set; }
        public object Data { get; set; }

        public static ReceiveMessage Subscribe(string path, ReceiveMessageStatusCode statusCode)
        {
            return new ReceiveMessage { Type = SubscribeType, Data = new SubscriptionMessageData { Path = path, StatusCode = statusCode } };
        }

        public static ReceiveMessage Unsubscribe(string path, ReceiveMessageStatusCode statusCode)
        {
            return new ReceiveMessage { Type = UnsubscribeType, Data = new SubscriptionMessageData { Path = path, StatusCode = statusCode } };
        }

        public static ReceiveMessage Changed(IEnumerable<ConfigKey> configKeys)
        {
            return new ReceiveMessage { Type = ChangedType, Data = configKeys };
        }

        public static ReceiveMessage Deleted(string path)
        {
            return new ReceiveMessage { Type = DeletedType, Data = path };
        }

        public override string ToString()
        {
            return Helper.JsonSerializer.Serialize(this);
        }
    }

    [JsonConverter(typeof(JsonReceiveSubscriptionMessageDataConverter))]
    internal struct SubscriptionMessageData
    {
        public string Path { get; set; }
        public ReceiveMessageStatusCode StatusCode { get; set; }
    }

    internal enum ReceiveMessageStatusCode
    {
        Success,
        Not_Found_KeyPath,
        Not_Found_App,
        Already_Exists
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
                    case JsonTokenType.StartObject:
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
                                        case ReceiveMessage.ChangedType:
                                            _data = JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(ref reader, options);
                                            _property = null;
                                            break;

                                        case ReceiveMessage.SubscribeType:
                                        case ReceiveMessage.UnsubscribeType:
                                            _data = JsonSerializer.Deserialize<SubscriptionMessageData>(ref reader, options);
                                            _property = null;
                                            break;

                                        case ReceiveMessage.DeletedType:
                                            _data = reader.GetString(); // Key path
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
                        switch (((string)reader.Value).ToLowerInvariant())
                        {
                            case "type":
                                _type = reader.ReadAsString().ToLowerInvariant();
                                break;

                            case "data":
                                switch (_type)
                                {
                                    case ReceiveMessage.ChangedType:
                                        if (reader.Read())
                                            _data = serializer.Deserialize<IEnumerable<ConfigKey>>(reader);
                                        break;

                                    case ReceiveMessage.SubscribeType:
                                    case ReceiveMessage.UnsubscribeType:
                                        if (reader.Read())
                                            _data = serializer.Deserialize<SubscriptionMessageData>(reader);
                                        break;

                                    case ReceiveMessage.DeletedType:
                                        _data = reader.ReadAsString(); // Key path
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


    internal class JsonReceiveSubscriptionMessageDataConverter : JsonConverter<SubscriptionMessageData>
    {
#if SJSON
        public override SubscriptionMessageData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string _property = null;
            string _path = null;
            ReceiveMessageStatusCode? _statusCode = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        _property = reader.GetString().ToLowerInvariant();
                        break;

                    case JsonTokenType.String:
                    case JsonTokenType.StartArray:
                    case JsonTokenType.StartObject:
                        if (_property != null)
                        {
                            switch (_property)
                            {
                                case "path":
                                    _path = reader.GetString();
                                    break;

                                case "statuscode":
                                    _statusCode = (ReceiveMessageStatusCode)Enum.Parse(typeof(ReceiveMessageStatusCode), reader.GetString(), true);
                                    break;
                            }
                        }
                        break;
                }
            }

            return new SubscriptionMessageData
            {
                Path = _path ?? throw new ArgumentException("Missing path"),
                StatusCode = _statusCode ?? throw new ArgumentException("Missing status code")
            };
        }

        public override void Write(Utf8JsonWriter writer, SubscriptionMessageData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("path", value.Path);
            writer.WriteString("statuscode", value.StatusCode.ToString().ToLowerInvariant());
            writer.WriteEndObject();
        }
#endif

#if NJSON
        public override SubscriptionMessageData ReadJson(JsonReader reader, Type objectType, SubscriptionMessageData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string _path = null;
            ReceiveMessageStatusCode? _statusCode = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (((string)reader.Value).ToLowerInvariant())
                        {
                            case "path":
                                _path = reader.ReadAsString().ToLowerInvariant();
                                break;

                            case "statuscode":
                                _statusCode = (ReceiveMessageStatusCode)Enum.Parse(typeof(ReceiveMessageStatusCode), reader.ReadAsString(), true);
                                break;
                        }
                        break;
                }
            }

            return new SubscriptionMessageData
            {
                Path = _path ?? throw new ArgumentException("Missing path"),
                StatusCode = _statusCode ?? throw new ArgumentException("Missing status code")
            };
        }

        public override void WriteJson(JsonWriter writer, SubscriptionMessageData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("path");
            writer.WriteValue(value.Path);
            writer.WritePropertyName("statuscode");
            serializer.Serialize(writer, value.StatusCode.ToString().ToLowerInvariant());
            writer.WriteEndObject();
        }
#endif
    }
}
