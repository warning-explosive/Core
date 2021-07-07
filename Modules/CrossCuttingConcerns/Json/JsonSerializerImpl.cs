namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSerializerImpl : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        [SuppressMessage("Analysis", "CA2326", Justification = "Custom SerializationBinder and CA2327")]
        public JsonSerializerImpl(
            ISerializationBinder serializationBinder,
            IEnumerable<JsonConverter> converters)
        {
            _settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            Formatting = Formatting.Indented,
                            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                            Converters = converters.ToList(),
                            SerializationBinder = serializationBinder
                        };
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, value.GetType(), _settings);
        }

        public object DeserializeObject(string serialized, Type type)
        {
            return JsonConvert.DeserializeObject(serialized, type, _settings)
                              .EnsureNotNull("Object should be serializable");
        }

        public T DeserializeObject<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized, _settings)
                              .EnsureNotNull<T>("Object should be serializable");
        }
    }
}