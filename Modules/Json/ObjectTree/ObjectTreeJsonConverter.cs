namespace SpaceEngineers.Core.Json.ObjectTree
{
    using System;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Internals;
    using Newtonsoft.Json;

    /// <summary>
    /// ObjectTreeJsonConverter
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public sealed class ObjectTreeJsonConverter : JsonConverter,
                                                  IJsonConverter
    {
        private readonly IObjectTreeValueReader _objectTreeValueReader;

        /// <summary> .cctor </summary>
        /// <param name="objectTreeValueReader">IObjectTreeValueReader</param>
        public ObjectTreeJsonConverter(IObjectTreeValueReader objectTreeValueReader)
        {
            _objectTreeValueReader = objectTreeValueReader;
        }

        /// <inheritdoc />
        public JsonConverter Converter => this;

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