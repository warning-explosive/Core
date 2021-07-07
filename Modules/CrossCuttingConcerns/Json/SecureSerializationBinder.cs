namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Converters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [Component(EnLifestyle.Singleton)]
    internal class SecureSerializationBinder : ISerializationBinder,
                                               IExternalResolvable<ISerializationBinder>
    {
        private readonly JsonSerializerSettings _settings;
        private readonly ITypeProvider _typeProvider;

        public SecureSerializationBinder(
            ITypeProvider typeProvider,
            TypeNodeJsonConverter typeNodeJsonConverter)
        {
            _settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            Converters = { typeNodeJsonConverter }
                        };

            _typeProvider = typeProvider;
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = JsonConvert.SerializeObject(new TypeNode(serializedType), _settings);
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            return TypeNode.Parse(typeName).BuildType(_typeProvider);
        }
    }
}