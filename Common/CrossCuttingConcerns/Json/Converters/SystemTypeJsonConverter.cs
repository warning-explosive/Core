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
    internal sealed class SystemTypeJsonConverter : JsonConverter<Type>,
                                                    IResolvable<SystemTypeJsonConverter>,
                                                    ICollectionResolvable<JsonConverter>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Type);
        }

        public override Type? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var str = reader.GetString();

            return str != null
                ? TypeNode.ToType(TypeNode.FromString(str))
                : null;
        }

        public override Type ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var str = reader.GetString();

            return str != null
                ? TypeNode.ToType(TypeNode.FromString(str))
                : throw new InvalidOperationException("Dictionary property name wasn't found");
        }

        public override void Write(
            Utf8JsonWriter writer,
            Type value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(TypeNode.FromType(value).ToString());
        }

        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            Type value,
            JsonSerializerOptions options)
        {
            writer.WritePropertyName(TypeNode.FromType(value).ToString());
        }
    }
}