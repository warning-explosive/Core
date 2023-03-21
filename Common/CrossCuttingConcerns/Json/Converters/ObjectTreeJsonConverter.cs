namespace SpaceEngineers.Core.CrossCuttingConcerns.Json.Converters
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Newtonsoft.Json;

    [Component(EnLifestyle.Singleton)]
    internal sealed class ObjectTreeJsonConverter : JsonConverter,
                                                    ICollectionResolvable<JsonConverter>
    {
        private readonly IObjectTreeValueReader _objectTreeValueReader;

        public ObjectTreeJsonConverter(IObjectTreeValueReader objectTreeValueReader)
        {
            _objectTreeValueReader = objectTreeValueReader;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException(nameof(WriteJson));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return new EnumerableObjectTreeReader(reader, _objectTreeValueReader).Last();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IObjectTreeNode);
        }
    }
}