namespace SpaceEngineers.Core.AutoRegistration
{
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
        /// Assemblies loaded in CurrentDomain and SpaceEngineers.Core.AutoWiringApi assembly as root assembly
        /// </summary>
        /// <param name="entryPointAssembly">Entry point assembly, should contains all references - application root</param>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(Assembly entryPointAssembly,
                                                  DependencyContainerOptions options)
        {
            AssembliesExtensions.WarmUpAppDomain(entryPointAssembly);

            var autoWiringApi = typeof(LifestyleAttribute).Assembly;

            return new DependencyContainerImpl(AssembliesExtensions.AllFromCurrentDomain(),
                                               new[] { autoWiringApi },
                                               options.RegistrationCallback);
        }
    }
}