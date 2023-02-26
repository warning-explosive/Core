namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using EqualityComparers;

    /// <summary>
    /// Assemblies extensions
    /// </summary>
    public static class AssembliesExtensions
    {
        private const string Dot = ".";

        private const string Duplicate = "xunit.runner.visualstudio.dotnetcore.testadapter";

        private static readonly string[] ExcludedAssemblies = new[]
        {
            nameof(System),
            nameof(Microsoft),
            "Windows"
        };

        private static readonly string[] RootAssemblies = new[]
        {
            "SpaceEngineers.Core.Basics",
            "SpaceEngineers.Core.AutoRegistration.Api",
            "SpaceEngineers.Core.CompositionRoot.Api",
            "SpaceEngineers.Core.CompositionRoot",

            "SpaceEngineers.Core.Analyzers.Api",
            "SpaceEngineers.Core.Benchmark.Api"
        };

        private static readonly Lazy<(Assembly[] OurAssemblies, Assembly[] AllAssemblies)> AllAssembliesLoadedInCurrentAppDomain
            = new Lazy<(Assembly[] OurAssemblies, Assembly[] AllAssemblies)>(WarmUpAppDomain, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Build name
        /// </summary>
        /// <param name="nameParts">Name parts</param>
        /// <returns>Built name</returns>
        public static string BuildName(params string[] nameParts)
        {
            return string.Join(Dot, nameParts);
        }

        /// <summary>
        /// Find required assembly in current AppDomain
        /// </summary>
        /// <param name="assemblyName">Assembly short name</param>
        /// <returns>Found assembly</returns>
        public static Assembly FindRequiredAssembly(string assemblyName)
        {
            return FindAssembly(assemblyName)
                .EnsureNotNull($"Assembly {assemblyName} should be found in current {nameof(AppDomain)}");
        }

        /// <summary>
        /// Find assembly in current AppDomain
        /// </summary>
        /// <param name="assemblyName">Assembly short name</param>
        /// <returns>Found assembly</returns>
        public static Assembly? FindAssembly(string assemblyName)
        {
            return AllAssembliesFromCurrentDomain()
                .SingleOrDefault(assembly => assembly.GetName().Name.Equals(assemblyName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all assemblies from current app domain
        /// </summary>
        /// <returns>All assemblies from current app domain</returns>
        public static Assembly[] AllAssembliesFromCurrentDomain()
        {
            return AllAssembliesLoadedInCurrentAppDomain.Value.AllAssemblies;
        }

        /// <summary>
        /// Get all our assemblies from current app domain
        /// </summary>
        /// <returns>All our assemblies from current app domain</returns>
        public static Assembly[] AllOurAssembliesFromCurrentDomain()
        {
            return AllAssembliesLoadedInCurrentAppDomain.Value.OurAssemblies;
        }

        /// <summary>
        /// Is our reference
        /// </summary>
        /// <param name="assemblies">All assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        /// <returns>Is our reference func</returns>
        public static Func<Assembly, bool> IsOurReference(
            IEnumerable<Assembly> assemblies,
            IEnumerable<Assembly> rootAssemblies)
        {
            var map = assemblies.ToDictionary(assembly => assembly.GetName().FullName);

            var visited = rootAssemblies
                .Distinct(new AssemblyByNameEqualityComparer())
                .ToDictionary(root => root.GetName().FullName, _ => true);

            return assembly => IsOurReferenceInternal(assembly, map, visited);

            static bool IsOurReferenceInternal(
                Assembly assembly,
                IReadOnlyDictionary<string, Assembly> map,
                IDictionary<string, bool> visited)
            {
                var key = string.Intern(assembly.GetName().FullName);

                if (visited.ContainsKey(key) && visited[key])
                {
                    return true;
                }

                var exclusiveReferences = assembly.GetReferencedAssemblies();

                var isReferencedDirectly = exclusiveReferences
                   .Any(assemblyName =>
                        visited.ContainsKey(assemblyName.FullName)
                        && visited[assemblyName.FullName]
                        && ExcludedAssemblies.All(ex => !assemblyName.FullName.StartsWith(ex, StringComparison.OrdinalIgnoreCase)));

                if (isReferencedDirectly)
                {
                    visited[key] = true;
                    return true;
                }

                var isIndirectlyReferenced = exclusiveReferences
                    .Where(unknownReference =>
                        !visited.ContainsKey(unknownReference.FullName)
                        && ExcludedAssemblies.All(ex => !unknownReference.FullName.StartsWith(ex, StringComparison.OrdinalIgnoreCase)))
                    .Any(unknownReference =>
                        map.TryGetValue(unknownReference.FullName, out var unknownAssembly)
                        && IsOurReferenceInternal(unknownAssembly, map, visited));

                visited[key] = isIndirectlyReferenced;
                return isIndirectlyReferenced;
            }
        }

        /// <summary>
        /// Get all assemblies referenced directly or indirectly to specified one
        /// </summary>
        /// <param name="allAssemblies">All assemblies</param>
        /// <param name="assembly">Bound assembly</param>
        /// <returns>All assemblies below bound includes bound</returns>
        public static Assembly[] Below(this Assembly[] allAssemblies, Assembly assembly)
        {
            var all = allAssemblies
                .Union(new[] { assembly })
                .Distinct(new AssemblyByNameEqualityComparer())
                .ToDictionary(a => string.Intern(a.GetName().FullName));

            var visited = new HashSet<string>();

            return BelowReference(assembly.GetName(), all, visited).ToArray();
        }

        private static IEnumerable<Assembly> BelowReference(
            AssemblyName assemblyName,
            IReadOnlyDictionary<string, Assembly> all,
            HashSet<string> visited)
        {
            var key = string.Intern(assemblyName.FullName);

            if (!visited.Add(key))
            {
                return Enumerable.Empty<Assembly>();
            }

            if (!all.TryGetValue(key, out var assembly))
            {
                return Enumerable.Empty<Assembly>();
            }

            return new[] { assembly }
                .Concat(assembly
                    .GetReferencedAssemblies()
                    .SelectMany(name => BelowReference(name, all, visited)));
        }

        private static (Assembly[] OurAssemblies, Assembly[] AllAssemblies) WarmUpAppDomain()
        {
            var loaded = new HashSet<string>();

            _ = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(AssemblyName.GetAssemblyName)
                .SelectMany(name => LoadReferences(name, loaded))
                .ToList();

            var allAssemblies = AppDomain.CurrentDomain
                .EnsureNotNull($"{nameof(AppDomain.CurrentDomain)} is null")
                .GetAssemblies()
                .GroupBy(assembly => assembly.GetName().Name)
                .SelectMany(RemoveDuplicates)
                .ToArray();

            var rootAssemblies = RootAssemblies
                .Select(optionalAssemblyName => allAssemblies
                    .SingleOrDefault(a => a.GetName().Name.Equals(optionalAssemblyName, StringComparison.OrdinalIgnoreCase)))
                .Where(optionalAssembly => optionalAssembly != null);

            var isOurReference = IsOurReference(allAssemblies, rootAssemblies);

            var ourAssemblies = allAssemblies.Where(isOurReference).ToArray();

            return (ourAssemblies, allAssemblies);
        }

        private static IEnumerable<Assembly> LoadReferences(AssemblyName assemblyName, HashSet<string> loaded)
        {
            var name = string.Intern(assemblyName.FullName);

            if (!loaded.Add(name))
            {
                return Enumerable.Empty<Assembly>();
            }

            if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
            {
                return Enumerable.Empty<Assembly>();
            }

            var assembly = LoadByName(assemblyName);

            if (assembly == null)
            {
                return Enumerable.Empty<Assembly>();
            }

            return new[] { assembly }
                .Concat(assembly
                    .GetReferencedAssemblies()
                    .SelectMany(referenceName => LoadReferences(referenceName, loaded)));
        }

        private static Assembly? LoadByName(AssemblyName assemblyName)
        {
            return ExecutionExtensions
                .Try<AssemblyName, Assembly?>(AppDomain.CurrentDomain.Load, assemblyName)
                .Catch<FileNotFoundException>()
                .Invoke(_ => default);
        }

        private static IEnumerable<Assembly> RemoveDuplicates(IGrouping<string, Assembly> grp)
        {
            if (grp.Key.Equals(Duplicate, StringComparison.OrdinalIgnoreCase))
            {
                yield return grp.First();
                yield break;
            }

            foreach (var item in grp)
            {
                yield return item;
            }
        }
    }
}