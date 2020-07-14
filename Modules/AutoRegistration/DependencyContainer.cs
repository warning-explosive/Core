namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;

    /// <summary>
    /// IDependencyContainer factory
    /// </summary>
    public static class DependencyContainer
    {
        /// <summary>
        /// Default configured container
        /// Assemblies loaded in CurrentDomain from BaseDirectory and SpaceEngineers.Core.AutoWiringApi assembly as root assembly
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(DependencyContainerOptions options)
        {
            AssembliesExtensions.WarmUpAppDomain(options.SearchOption);

            var autoWiringApi = typeof(LifestyleAttribute).Assembly;

            return new DependencyContainerImpl(AssembliesExtensions.AllFromCurrentDomain(),
                                               new[] { autoWiringApi },
                                               options?.RegistrationCallback);
        }

        /// <summary>
        /// Container configured bounded by specified assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBounded(Assembly[] assemblies, DependencyContainerOptions options)
        {
            var autoWiringApi = typeof(LifestyleAttribute).Assembly;

            return new DependencyContainerImpl(assemblies,
                                               new[] { autoWiringApi },
                                               options.RegistrationCallback);
        }
    }
}