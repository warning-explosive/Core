namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class TypeInfoStorage : ITypeInfoStorage
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> _collection = new ConcurrentDictionary<Type, TypeInfo>();

        public TypeInfoStorage()
        {
            var rootAssemblyName = typeof(DependencyContainer).Assembly.GetName();
            
            OurAssemblies = AppDomain.CurrentDomain
                                     .GetAssemblies()
                                     .AsParallel()
                                     //TODO .Where(assembly => assembly.GetReferencedAssemblies().Contains(rootAssemblyName))
                                     .ToArray();

            OurTypes = OurAssemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();
        }

        public TypeInfo this[Type type] => _collection.GetOrAdd(type, t => new TypeInfo(t));

        public Assembly[] OurAssemblies { get; }

        public Type[] OurTypes { get; }
    }
}