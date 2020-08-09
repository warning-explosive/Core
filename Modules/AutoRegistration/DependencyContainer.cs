namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using Internals;

    /// <summary>
    /// IDependencyContainer factory
    /// </summary>
    public static class DependencyContainer
    {
        private static readonly Assembly[] RootAssemblies =
        {
            typeof(LifestyleAttribute).Assembly, // AutoWiringAPI
            typeof(TypeExtensions).Assembly,     // Basics
        };

        /// <summary>
        /// Default configured container
        /// Assemblies loaded in CurrentDomain from BaseDirectory and SpaceEngineers.Core.AutoWiringApi assembly as root assembly
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(DependencyContainerOptions options)
        {
            AssembliesExtensions.WarmUpAppDomain(options.SearchOption);

            var typeProvider = new ContainerDependentTypeProvider(AssembliesExtensions.AllFromCurrentDomain(), RootAssemblies);

            var dependencyContainer = new DependencyContainerImpl(typeProvider, options);

            return dependencyContainer;
        }

        /// <summary>
        /// Container configured bounded by specified assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBounded(Assembly[] assemblies, DependencyContainerOptions options)
        {
            var typeProvider = new ContainerDependentTypeProvider(assemblies, RootAssemblies);

            var dependencyContainer = new DependencyContainerImpl(typeProvider, options);

            return dependencyContainer;
        }
    }
}