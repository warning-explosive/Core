namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Extensions;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class TypeInfoStorage : ITypeInfoStorage
    {
        private readonly IDictionary<Guid, TypeInfo> _collection = new ConcurrentDictionary<Guid, TypeInfo>();

        public TypeInfoStorage()
        {
            var rootAssembly = typeof(DependencyContainer).Assembly;
            var rootAssemblyFullName = rootAssembly.GetName().FullName;

            OurAssemblies = AppDomain.CurrentDomain
                                     .GetAssemblies()
                                     .AsParallel()
                                     .Where(z => z.GetReferencedAssemblies().Select(x => x.FullName).Contains(rootAssemblyFullName))
                                     .ToArray()
                                     .Concat(new[] { rootAssembly })
                                     .ToArray();

            OurTypes = OurAssemblies.SelectMany(assembly => assembly.GetTypes()
                                                                    .Where(z => z.FullName != null
                                                                                && !z.FullName.Contains("+<>c")))
                                    .ToArray();

            OurTypes.Each(type => _collection.Add(type.GUID, new TypeInfo(type)));
        }

        public TypeInfo this[Type type]
            => _collection.TryGetValue(type.GUID, out var typeInfo)
                   ? typeInfo
                   : throw new KeyNotFoundException(type.FullName);

        public Assembly[] OurAssemblies { get; }

        public Type[] OurTypes { get; }
    }
}