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
        private const string Duplicate = "xunit.runner.visualstudio.dotnetcore.testadapter";

        private static int _alreadyWarmedUp;

        /// <summary>
        /// Get all assemblies from current domain
        /// </summary>
        /// <returns>All assemblies from current domain</returns>
        public static Assembly[] AllFromCurrentDomain()
        {
            return AppDomain.CurrentDomain
                            .EnsureNotNull($"{nameof(AppDomain.CurrentDomain)} is null")
                            .GetAssemblies()
                            .GroupBy(assembly => assembly.GetName().Name)
                            .SelectMany(RemoveDuplicates)
                            .ToArray();

            IEnumerable<Assembly> RemoveDuplicates(IGrouping<string, Assembly> grp)
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

        /// <summary>
        /// Load all referenced assemblies into AppDomain
        /// </summary>
        /// <param name="searchOption">SearchOption for assemblies in BaseDirectory</param>
        public static void WarmUpAppDomain(SearchOption searchOption)
        {
            var previous = Interlocked.Exchange(ref _alreadyWarmedUp, 1);

            if (previous == default)
            {
                var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                _ = Directory
                    .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", searchOption)
                    .Select(AssemblyName.GetAssemblyName)
                    .SelectMany(name => LoadReferences(name, loaded))
                    .ToList();
            }

            static Assembly? LoadByName(AssemblyName assemblyName)
            {
                return ExecutionExtensions
                    .Try(() => AppDomain.CurrentDomain.Load(assemblyName))
                    .Catch<FileNotFoundException>()
                    .Invoke();
            }

            static IEnumerable<Assembly> LoadReferences(AssemblyName assemblyName, HashSet<string> loaded)
            {
                if (loaded.Contains(assemblyName.FullName))
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

                loaded.Add(assemblyName.FullName);

                return new[] { assembly }
                    .Concat(assembly
                        .GetReferencedAssemblies()
                        .SelectMany(referenceName => LoadReferences(referenceName, loaded)));
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
                .ToDictionary(a => a.GetName().FullName, StringComparer.OrdinalIgnoreCase);

            return BelowReference(assembly.GetName(), all).ToArray();

            static IEnumerable<Assembly> BelowReference(
                AssemblyName assemblyName,
                IReadOnlyDictionary<string, Assembly> all)
            {
                var key = assemblyName.FullName;

                if (!all.TryGetValue(key, out var assembly))
                {
                    return Enumerable.Empty<Assembly>();
                }

                return new[] { assembly }
                    .Concat(assembly
                        .GetReferencedAssemblies()
                        .SelectMany(name => BelowReference(name, all)));
            }
        }
    }
}