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
        private static readonly Assembly[] RootAssemblies = new[]
                                                            {
                                                                typeof(LifestyleAttribute).Assembly, // AutoWiringAPI
                                                                typeof(ITypeExtensions).Assembly,    // Basics
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

            var typeExtensions = TypeExtensions.Build(AssembliesExtensions.AllFromCurrentDomain(), RootAssemblies);

            TypeExtensions.Configure(() => typeExtensions);

            var dependencyContainer = new DependencyContainerImpl(typeExtensions, options.RegistrationCallback);

            TypeExtensions.Configure(() => dependencyContainer.Resolve<ITypeExtensions>());

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
            var typeExtensions = TypeExtensions.Build(assemblies, RootAssemblies);

            TypeExtensions.Configure(() => typeExtensions);

            var dependencyContainer = new DependencyContainerImpl(typeExtensions, options.RegistrationCallback);

            TypeExtensions.Configure(() => dependencyContainer.Resolve<ITypeExtensions>());

            return dependencyContainer;
        }
    }
}