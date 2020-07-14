namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Assemblies extensions
    /// </summary>
    public static class AssembliesExtensions
    {
        /// <summary>
        /// Get all assemblies from current domain
        /// </summary>
        /// <returns>All assemblies from current domain</returns>
        public static Assembly[] AllFromCurrentDomain()
        {
            return AppDomain.CurrentDomain
                            .EnsureNotNull(() => new InvalidOperationException($"{nameof(AppDomain.CurrentDomain)} is null"))
                            .GetAssemblies();
        }

        /// <summary>
        /// Load all referenced assemblies into AppDomain
        /// </summary>
        /// <param name="searchOption">SearchOption for assemblies in BaseDirectory</param>
        public static void WarmUpAppDomain(SearchOption searchOption)
        {
            /*
             * 1 - load from directory
             * 2 - load by referenced assemblies
             */

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