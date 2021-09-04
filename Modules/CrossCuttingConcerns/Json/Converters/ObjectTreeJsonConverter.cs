namespace SpaceEngineers.Core.CrossCuttingConcerns.Json.Converters
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Newtonsoft.Json;

    /// <summary>
    /// ObjectTreeJsonConverter
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public sealed class ObjectTreeJsonConverter : JsonConverter,
                                                  ICollectionResolvable<JsonConverter>
    {
        private readonly IObjectTreeValueReader _objectTreeValueReader;

        /// <summary> .cctor </summary>
        /// <param name="objectTreeValueReader">IObjectTreeValueReader</param>
        public ObjectTreeJsonConverter(IObjectTreeValueReader objectTreeValueReader)
        {
            _objectTreeValueReader = objectTreeValueReader;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException(nameof(WriteJson));
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return new EnumerableObjectTreeReader(reader, _objectTreeValueReader).Last();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IObjectTreeNode);
        }
    }
}