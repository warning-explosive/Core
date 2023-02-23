namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSerializerImpl : IJsonSerializer,
                                        IResolvable<IJsonSerializer>
    {
        private readonly JsonSerializerSettings _settings;

        [SuppressMessage("Analysis", "CA2326", Justification = "custom serializationBinder")]
        [SuppressMessage("Analysis", "CA2328", Justification = "custom serializationBinder")]
        public JsonSerializerImpl(
            ISerializationBinder serializationBinder,
            IEnumerable<JsonConverter> converters)
        {
            _settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                            Formatting = Formatting.Indented,
                            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                            Converters = converters.ToList(),
                            SerializationBinder = serializationBinder
                        };
        }

        public string SerializeObject(object value, Type type)
        {
            return JsonConvert.SerializeObject(value, type, _settings);
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