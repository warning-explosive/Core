namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class TypeInfoStorage : ITypeInfoStorage
    {
        private static readonly string[] _excluded =
        {
            "<>f",
            "<>c",
            "<PrivateImplementationDetails>",
            nameof(Microsoft),
            nameof(System),
        };

        private const string Utilities = "SpaceEngineers.Core.Utilities";
        
        private readonly IDictionary<Guid, TypeInfo> _collection = new Dictionary<Guid, TypeInfo>();

        public TypeInfoStorage(Assembly[] assemblies)
        {
            var rootAssembly = typeof(TypeExtensions).Assembly;
            
            var rootAssemblyFullName = rootAssembly.GetName().FullName;

            OurAssemblies = assemblies
                           .Where(z => z.GetReferencedAssemblies()
                                        .Select(x => x.FullName)
                                        .Contains(rootAssemblyFullName)
                                       && z.GetName().Name != Utilities)
                           .ToArray()
                           .Concat(new[] { rootAssembly })
                           .ToArray();

            OurTypes = OurAssemblies.SelectMany(assembly => assembly.GetTypes()
                                                                    .Where(z => z.FullName != null
                                                                                && _excluded.All(mask => !z.FullName.Contains(mask))))
                                    .ToArray();

            OurTypes.Each(type => _collection.Add(type.GUID, new TypeInfo(type)));

            AllLoadedTypes = assemblies.SelectMany(z => z.GetTypes())
                                       .ToArray();
        }

        public TypeInfo this[Type type]
            => _collection.ContainsKey(type.GUID)
                   ? _collection[type.GUID]
                   : new TypeInfo(type);

        public bool ContainsKey(Type type)
        {
            return _collection.ContainsKey(type.GUID);
        }

        public Assembly[] OurAssemblies { get; }

        public Type[] OurTypes { get; }

        public Type[] AllLoadedTypes { get; }
    }
}