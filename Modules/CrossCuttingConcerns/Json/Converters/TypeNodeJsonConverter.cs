namespace SpaceEngineers.Core.CrossCuttingConcerns.Json.Converters
{
    using System;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Newtonsoft.Json;

    [Component(EnLifestyle.Singleton)]
    internal class TypeNodeJsonConverter : JsonConverter
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
                       ? TypeNode.Parse(str)
                       : null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TypeNode);
        }
    }
}