namespace SpaceEngineers.Core.NewtonSoft.Json.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using Basics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class SecureSerializationBinder : ISerializationBinder
    {
        private readonly JsonSerializerSettings _settings;
        private readonly ITypeProvider _typeProvider;

        public SecureSerializationBinder(ITypeProvider typeProvider)
        {
            _settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            Converters = { new TypeNodeJsonConverter() }
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