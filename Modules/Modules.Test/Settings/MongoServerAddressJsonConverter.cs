namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using NewtonSoft.Json.Abstractions;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class MongoServerAddressJsonConverter : JsonConverter,
                                                     IJsonConverter
    {
        public JsonConverter Converter => this;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value is MongoServerAddress address)
            {
                writer.WritePropertyName(nameof(MongoServerAddress.Host));
                writer.WriteValue(address.Host);
                writer.WritePropertyName(nameof(MongoServerAddress.Port));
                writer.WriteValue(address.Port);
            }
            else
            {
                writer.WriteNull();
            }

            writer.WriteEndObject();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var properties = new Dictionary<string, object?>();
            var key = string.Empty;

            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        key = (string)reader.Value!;
                        reader.Read();
                        break;
                    case JsonToken.String:
                        properties[key] = reader.Value;
                        reader.Read();
                        break;
                    default:
                        reader.Read();
                        break;
                }
            }

            if (existingValue != null)
            {
                return existingValue;
            }

            var address = string.Join(":", properties.Values);
            return MongoServerAddress.Parse(address);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MongoServerAddress);
        }
    }
}