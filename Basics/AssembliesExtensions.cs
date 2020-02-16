namespace SpaceEngineers.Core.Basics
{
    using System;
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
    }
}