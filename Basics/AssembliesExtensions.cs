namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Assemblies extensions
    /// </summary>
    public static class AssembliesExtensions
    {
        private const string Duplicate = "xunit.runner.visualstudio.dotnetcore.testadapter";

        /// <summary>
        /// Get all assemblies from current domain
        /// </summary>
        /// <returns>All assemblies from current domain</returns>
        public static Assembly[] AllFromCurrentDomain()
        {
            return AppDomain.CurrentDomain
                            .EnsureNotNull(() => new InvalidOperationException($"{nameof(AppDomain.CurrentDomain)} is null"))
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
            Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", searchOption)
                     .Select(Load)
                     .SelectMany(assembly => assembly.GetReferencedAssemblies())
                     .Distinct()
                     .Where(a => a.ContentType != AssemblyContentType.WindowsRuntime)
                     .Each(assembly => AppDomain.CurrentDomain.Load(assembly));

            Assembly Load(string library) => AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(library));
        }
    }
}