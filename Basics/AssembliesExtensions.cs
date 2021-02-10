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
                if (grp.Key == Duplicate)
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
                Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", searchOption)
                    .Select(Load)
                    .SelectMany(assembly => assembly.GetReferencedAssemblies())
                    .Distinct()
                    .Where(a => a.ContentType != AssemblyContentType.WindowsRuntime)
                    .Each(assembly => AppDomain.CurrentDomain.Load(assembly));
            }

            static Assembly Load(string library) => AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(library));
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
                .ToDictionary(a => a.GetName().FullName);

            return new[] { assembly }
                .Concat(BelowReference(assembly, all))
                .Distinct(new AssemblyByNameEqualityComparer())
                .ToArray();

            static IEnumerable<Assembly> BelowReference(Assembly assembly, IReadOnlyDictionary<string, Assembly> all)
            {
                var references = assembly
                    .GetReferencedAssemblies()
                    .Where(reference => all.ContainsKey(reference.FullName))
                    .Select(reference => all[reference.FullName])
                    .ToList();

                return references.Concat(references.SelectMany(reference => BelowReference(reference, all)));
            }
        }
    }
}