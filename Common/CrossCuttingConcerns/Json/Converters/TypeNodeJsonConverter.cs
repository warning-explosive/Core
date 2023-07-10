namespace SpaceEngineers.Core.CrossCuttingConcerns.Json.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal sealed class TypeNodeJsonConverter : JsonConverter<TypeNode>,
                                                  IResolvable<TypeNodeJsonConverter>,
                                                  ICollectionResolvable<JsonConverter>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TypeNode);
        }

        public override TypeNode? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var str = reader.GetString();

            return str != null
                ? TypeNode.FromString(str)
                : null;
        }

        public override TypeNode ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var str = reader.GetString();

            return str != null
                ? TypeNode.FromString(str)
                : throw new InvalidOperationException("Dictionary property name wasn't found");
        }

        public override void Write(
            Utf8JsonWriter writer,
            TypeNode value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            TypeNode value,
            JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }
}