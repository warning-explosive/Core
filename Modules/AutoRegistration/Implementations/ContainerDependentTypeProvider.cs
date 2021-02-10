namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Services;
    using Basics.EqualityComparers;

    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
    [ManualRegistration]
    internal class ContainerDependentTypeProvider : ITypeProvider
    {
        private static readonly string[] ExcludedAssemblies =
        {
            nameof(System),
            nameof(Microsoft),
            "Windows",
        };

        private static readonly string[] ExcludedTypes =
        {
            "<>f",
            "<>c",
            "d__",
            "<PrivateImplementationDetails>",
            "AutoGeneratedProgram",
            "Xunit",
            "System.Runtime.CompilerServices",
            "Microsoft.CodeAnalysis",
        };

        private readonly HashSet<string> _ourTypesCache;

        public ContainerDependentTypeProvider(Assembly[] allAssemblies,
                                              Assembly[] rootAssemblies,
                                              IReadOnlyCollection<string> excludedNamespaces)
        {
            AllLoadedAssemblies = allAssemblies
                .Union(rootAssemblies)
                .Distinct(new AssemblyByNameEqualityComparer())
                .Where(a => !a.IsDynamic)
                .ToList();

            AllLoadedTypes = AllLoadedAssemblies
                .SelectMany(a => a.GetTypes())
                .ToList();

            TypeCache = AllLoadedAssemblies
                       .ToDictionary(assembly => assembly.GetName().Name,
                                     assembly => (IReadOnlyDictionary<string, Type>)assembly.GetTypes().ToDictionary(type => type.FullName));

            var allAssembliesDict = AllLoadedAssemblies.ToDictionary(a => a.GetName().FullName);

            var visited = rootAssemblies
                .Distinct(new AssemblyByNameEqualityComparer())
                .ToDictionary(root => root.GetName().FullName, _ => true);

            OurAssemblies = allAssembliesDict
                           .Select(pair => pair.Value)
                           .Where(a => IsOurReference(a, allAssembliesDict, visited))
                           .ToList();

            OurTypes = OurAssemblies
                      .SelectMany(ExtractOurTypes)
                      .Where(t => !excludedNamespaces.Contains(t.Namespace, StringComparer.InvariantCulture))
                      .ToList();

            _ourTypesCache = new HashSet<string>(OurTypes.Select(type => type.FullName));
        }

        public IReadOnlyCollection<Assembly> AllLoadedAssemblies { get; }

        public IReadOnlyCollection<Type> AllLoadedTypes { get; }

        public IReadOnlyCollection<Assembly> OurAssemblies { get; }

        public IReadOnlyCollection<Type> OurTypes { get; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TypeCache { get; }

        public bool IsOurType(Type type)
        {
            return _ourTypesCache.Contains(type.FullName);
        }

        private bool IsOurReference(Assembly assembly, IReadOnlyDictionary<string, Assembly> all, IDictionary<string, bool> visited)
        {
            var key = assembly.GetName().FullName;

            if (visited.ContainsKey(key) && visited[key])
            {
                return true;
            }

            var exclusiveReferences = assembly.GetReferencedAssemblies();
            var isReferencedDirectly = exclusiveReferences.Any(a => visited.ContainsKey(a.FullName) && visited[a.FullName]);

            if (isReferencedDirectly)
            {
                visited[key] = true;
                return true;
            }

            var isIndirectlyReferenced = exclusiveReferences
                .Where(unknownReference => !visited.ContainsKey(unknownReference.FullName)
                                        && ExcludedAssemblies.All(ex => !unknownReference.FullName.StartsWith(ex, StringComparison.InvariantCultureIgnoreCase)))
                .Any(unknownReference => all.TryGetValue(unknownReference.FullName, out var unknownAssembly)
                                         && IsOurReference(unknownAssembly, all, visited));

            visited[key] = isIndirectlyReferenced;
            return isIndirectlyReferenced;
        }

        private IEnumerable<Type> ExtractOurTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(t => t.FullName != null
                                    && ExcludedTypes.All(mask => !t.FullName.Contains(mask, StringComparison.InvariantCulture)));
        }
    }
}