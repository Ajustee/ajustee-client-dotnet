﻿using System;
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

        public string Action { get; set; }

        public object Data { get; set; }

        #endregion
    }

    internal class JsonReceiveMessageConverter : JsonConverter<ReceiveMessage>
    {
#if SJSON
        public override ReceiveMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string _property = null;
            string _action = null;
            object _data = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        _property = reader.GetString().ToLowerInvariant();
                        break;

                    default:
                        if (_property != null)
                        {
                            switch (_property)
                            {
                                case "action":
                                    _action = reader.GetString().ToLowerInvariant();
                                    break;

                                case "data":
                                    switch (_action)
                                    {
                                        case ReceiveMessage.ConfigKeys:
                                            _data = JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(ref reader, options);
                                            _property = null;
                                            break;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            return new ReceiveMessage { Action = _action, Data = _data };
        }

        public override void Write(Utf8JsonWriter writer, ReceiveMessage value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
#endif

#if NJSON
        public override ReceiveMessage ReadJson(JsonReader reader, Type objectType, ReceiveMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string _action = null;
            object _data = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (reader.Path.ToLowerInvariant())
                        {
                            case "action":
                                _action = reader.ReadAsString().ToLowerInvariant();
                                break;

                            case "data":
                                switch (_action)
                                {
                                    case ReceiveMessage.ConfigKeys:
                                        if (reader.Read())
                                            _data = serializer.Deserialize<IEnumerable<ConfigKey>>(reader);
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }

            return new ReceiveMessage { Action = _action, Data = _data };
        }

        public override void WriteJson(JsonWriter writer, ReceiveMessage value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
#endif
    }
}