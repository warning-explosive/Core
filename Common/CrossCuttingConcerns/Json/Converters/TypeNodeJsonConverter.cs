namespace SpaceEngineers.Core.CrossCuttingConcerns.Json.Converters
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Newtonsoft.Json;

    [Component(EnLifestyle.Singleton)]
    internal class TypeNodeJsonConverter : JsonConverter,
                                           IResolvable<TypeNodeJsonConverter>,
                                           ICollectionResolvable<JsonConverter>
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is TypeNode typeNode)
            {
                writer.WriteRaw(typeNode.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var str = reader.ReadAsString();

            return str != null
                ? TypeNode.FromString(str)
                : null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TypeNode);
        }
    }
}