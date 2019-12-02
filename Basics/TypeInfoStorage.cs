namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using EqualityComparers;

    internal class TypeInfoStorage : ITypeInfoStorage
    {
        private static readonly string[] _excludedAssemblies =
        {
            nameof(System),
            nameof(Microsoft),
            "Windows",
        };
        
        private static readonly string[] _excludedTypes =
        {
            "<>f",
            "<>c",
            "<PrivateImplementationDetails>",
            "AutoGeneratedProgram",
            nameof(Microsoft),
            nameof(System),
            "Windows",
        };
        
        private readonly IDictionary<Guid, TypeInfo> _collection = new Dictionary<Guid, TypeInfo>();

        public TypeInfoStorage(Assembly[] assemblies)
        {
            var rootAssembly = typeof(TypeExtensions).Assembly;
            var rootAssemblyFullName = rootAssembly.GetName().FullName;

            var loadedAssemblies = assemblies.Distinct(new AssemblyByNameEqualityComparer())
                                             .ToDictionary(a => a.GetName().FullName);

            var visited = loadedAssemblies.Where(a => _excludedAssemblies.Any(ex => a.Key.StartsWith(ex)))
                                          .ToDictionary(k => k.Key, v => false);

            visited[rootAssemblyFullName] = true;
            
            OurAssemblies = loadedAssemblies
                           .Except(new[]
                                   {
                                       new KeyValuePair<string, Assembly>(rootAssemblyFullName, rootAssembly)
                                   })
                           .Where(pair => IsOurReference(pair.Value, loadedAssemblies, visited))
                           .Select(pair => pair.Value)
                           .Concat(new[] { rootAssembly })
                           .ToArray();

            OurTypes = OurAssemblies.SelectMany(assembly => assembly.GetTypes()
                                                                    .Where(t => t.FullName != null
                                                                                && _excludedTypes.All(mask => !t.FullName.Contains(mask))))
                                    .ToArray();

            OurTypes.Each(type => _collection.Add(type.GUID, new TypeInfo(type)));

            AllLoadedTypes = assemblies.SelectMany(a => a.GetTypes())
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

        private bool IsOurReference(Assembly assembly,
                                    IReadOnlyDictionary<string, Assembly> loadedAssemblies,
                                    IDictionary<string, bool> visited)
        {
            var exclusiveReferences = assembly.GetReferencedAssemblies();

            var isReferenced = exclusiveReferences.Any(a => visited.ContainsKey(a.FullName)
                                                            && visited[a.FullName]);
            
            var key = assembly.GetName().FullName;
            
            if (isReferenced)
            {
                visited[key] = true;
                
                return true;
            }

            var result = exclusiveReferences.Where(unknownReference => !visited.ContainsKey(unknownReference.FullName)
                                                                       && _excludedAssemblies.All(ex => !unknownReference.FullName.StartsWith(ex)))
                                            .Any(unknownReference => loadedAssemblies.TryGetValue(unknownReference.FullName, out var unknownAssembly)
                                                                     && IsOurReference(unknownAssembly, loadedAssemblies, visited));

            visited[key] = result;
            
            return result;
        }
    }
}