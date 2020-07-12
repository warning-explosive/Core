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
        /// <param name="entryPointAssembly">Entry point assembly, should contains all references - application root</param>
        public static void WarmUpAppDomain(Assembly entryPointAssembly)
        {
            // 1 - load by referenced assemblies
            foreach (var assembly in entryPointAssembly.GetReferencedAssemblies()
                                                       .Where(a => a.ContentType != AssemblyContentType.WindowsRuntime))
            {
                AppDomain.CurrentDomain.Load(assembly);
            }

            // 2 - load from directory
            var libraries = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var library in libraries)
            {
                try
                {
                    AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(library));
                }
                catch (FileNotFoundException)
                {
                }
            }
        }
    }
}