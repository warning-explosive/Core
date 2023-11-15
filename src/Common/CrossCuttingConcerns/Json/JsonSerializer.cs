namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Exceptions;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSerializer : IJsonSerializer,
                                    IResolvable<IJsonSerializer>
    {
        internal const string MetadataKey = "$";

        private static readonly PropertyInfo JsonNodeParent = typeof(JsonNode)
            .GetProperty(nameof(JsonNode.Parent), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty)
            ?? throw new NotFoundException($"Unable to find '{nameof(JsonNode.Parent)}' property in the {typeof(JsonNode)} type");

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
            // TODO: #217 - https://github.com/dotnet/runtime/issues/72604
            serialized = ReorderMetadata(JsonNode.Parse(serialized)).ToJsonString();

            return System.Text.Json.JsonSerializer.Deserialize(serialized, type, _options)
                   ?? throw new InvalidOperationException("Object should be serializable");
        }

        public T DeserializeObject<T>(string serialized)
        {
            return (T)DeserializeObject(serialized, typeof(T));
        }

        private JsonNode? ReorderMetadata(JsonNode? node)
        {
            return node switch
            {
                JsonObject jsonObject => ReorderJsonObject(jsonObject),
                JsonArray jsonArray => ReorderJsonArray(jsonArray),
                _ => node
            };

            JsonNode ReorderJsonObject(JsonObject jsonObject)
            {
                var properties = jsonObject
                    .Select(property =>
                    {
                        var value = ReorderMetadata(property.Value);

                        if (value != null)
                        {
                            JsonNodeParent.SetValue(value, null);
                        }

                        return new KeyValuePair<string, JsonNode?>(property.Key, value);
                    })
                    .OrderByDescending(property => property.Key.StartsWith(MetadataKey));

                var jsonNodeOptions = new JsonNodeOptions
                {
                    PropertyNameCaseInsensitive = _options.PropertyNameCaseInsensitive
                };

                return new JsonObject(properties, jsonNodeOptions);
            }

            JsonNode ReorderJsonArray(JsonArray jsonArray)
            {
                var items = jsonArray
                    .Select(item =>
                    {
                        var value = ReorderMetadata(item);

                        if (value != null)
                        {
                            JsonNodeParent.SetValue(value, null);
                        }

                        return value;
                    })
                    .ToArray();

                var jsonNodeOptions = new JsonNodeOptions
                {
                    PropertyNameCaseInsensitive = _options.PropertyNameCaseInsensitive
                };

                return new JsonArray(jsonNodeOptions, items);
            }
        }
    }
}