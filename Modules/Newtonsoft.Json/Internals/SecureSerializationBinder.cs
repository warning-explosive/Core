namespace SpaceEngineers.Core.NewtonSoft.Json.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class SecureSerializationBinder : ISerializationBinder
    {
        private readonly JsonSerializerSettings _settings;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> _associations;

        public SecureSerializationBinder(ITypeExtensions typeExtensions)
        {
            _settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            Converters = { new TypeNodeJsonConverter() }
                        };

            // TODO: cache it in provider
            _associations = typeExtensions
                           .AllLoadedTypes()
                           .GroupBy(type => type.Assembly.GetName().Name)
                           .ToDictionary(grp => grp.Key,
                                         grp => (IReadOnlyDictionary<string, Type>)grp.ToDictionary(type => type.FullName));
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = JsonConvert.SerializeObject(new TypeNode(serializedType), _settings);
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            return TypeNode.Parse(typeName).BuildType(_associations);
        }
    }
}