namespace SpaceEngineers.Core.Basics
{
    using System;
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
                            .TryExtractFromNullable(() => new InvalidOperationException($"{nameof(AppDomain.CurrentDomain)} is null"))
                            .GetAssemblies();
        }

        /// <summary>
        /// Load all referenced assemblies into AppDomain
        /// </summary>
        /// <param name="entryPointAssembly">Entry point assembly, should contains all references - application root</param>
        public static void WarmUpAppDomain(Assembly entryPointAssembly)
        {
            foreach (var assembly in entryPointAssembly.GetReferencedAssemblies()
                                                        .Where(a => a.ContentType != AssemblyContentType.WindowsRuntime))
            {
                AppDomain.CurrentDomain.Load(assembly);
            }
        }
    }
}