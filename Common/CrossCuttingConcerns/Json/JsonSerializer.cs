namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSerializer : IJsonSerializer,
                                    IResolvable<IJsonSerializer>
    {
        private readonly JsonSerializerOptions _options;

        [SuppressMessage("Analysis", "CA2326", Justification = "custom serializationBinder")]
        [SuppressMessage("Analysis", "CA2328", Justification = "custom serializationBinder")]
        public JsonSerializer(
            IJsonTypeInfoResolver typeInfoResolver,
            IEnumerable<JsonConverter> converters)
        {
            _options = new JsonSerializerOptions
            {
                TypeInfoResolver = typeInfoResolver,

                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.Strict,
                ReadCommentHandling = JsonCommentHandling.Disallow,
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
                ReferenceHandler = ReferenceHandler.Preserve,

                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = false,
                IgnoreReadOnlyFields = false,
                AllowTrailingCommas = false,
                IncludeFields = false,
                WriteIndented = false,

                MaxDepth = 64

                // TODO: #217
                // Encoder =,
                // DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                // PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            foreach (var converter in converters)
            {
                _options.Converters.Add(converter);
            }
        }

        public string SerializeObject(object value, Type type)
        {
            return System.Text.Json.JsonSerializer.Serialize(value, type, _options);
        }

        public object DeserializeObject(string serialized, Type type)
        {
            type = GetType(serialized, type);

            return System.Text.Json.JsonSerializer.Deserialize(serialized, type, _options)
                   ?? throw new InvalidOperationException("Object should be serializable");
        }

        public T DeserializeObject<T>(string serialized)
        {
            return (T)DeserializeObject(serialized, typeof(T));
        }

        private static Type GetType(string serialized, Type type)
        {
            using (var document = JsonDocument.Parse(serialized))
            {
                Type serializedType = document.RootElement.ValueKind == JsonValueKind.Object
                                      && document.RootElement.TryGetProperty(PolymorphicJsonTypeInfoResolver.TypeDiscriminatorPropertyName, out var typeDiscriminatorProperty)
                                      && typeDiscriminatorProperty.ValueKind != JsonValueKind.Undefined
                                      && typeDiscriminatorProperty.GetString() is { } typeDiscriminator
                                      && !typeDiscriminator.IsNullOrEmpty()
                    ? TypeNode.FromString(typeDiscriminator)
                    : type;

                if (!type.IsAssignableFrom(serializedType))
                {
                    throw new InvalidOperationException($"Unable to deserialize {serializedType} as {type}");
                }

                return serializedType;
            }
        }
    }
}