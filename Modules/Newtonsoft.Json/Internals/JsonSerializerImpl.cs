namespace SpaceEngineers.Core.NewtonSoft.Json.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Newtonsoft.Json;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class JsonSerializerImpl : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        [SuppressMessage("Microsoft.Security", "CA2326", Justification = "Custom SerializationBinder and CA2327")]
        public JsonSerializerImpl(ITypeExtensions typeExtensions,
                                  IEnumerable<IJsonConverter> converters)
        {
            _settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            Formatting = Formatting.Indented,
                            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                            Converters = converters.Select(c => c.Converter).ToList(),
                            SerializationBinder = new SecureSerializationBinder(typeExtensions)
                        };
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, value.GetType(), _settings);
        }

        public object DeserializeObject(string serialized, Type type)
        {
            return JsonConvert.DeserializeObject(serialized, type, _settings)
                              .EnsureNotNull("Object must be serializable");
        }

        public T DeserializeObject<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized, _settings)
                              .EnsureNotNull<T>("Object must be serializable");
        }
    }
}