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
    internal sealed class SystemTypeNodeJsonConverter : JsonConverter<Type>,
                                                        IResolvable<SystemTypeNodeJsonConverter>,
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

        public override void Write(
            Utf8JsonWriter writer,
            Type value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(TypeNode.FromType(value).ToString());
        }
    }
}