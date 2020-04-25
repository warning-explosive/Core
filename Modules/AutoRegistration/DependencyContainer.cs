namespace SpaceEngineers.Core.AutoRegistration
{
    using System.Reflection;
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
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Default(Assembly entryPointAssembly)
        {
            AssembliesExtensions.WarmUpAppDomain(entryPointAssembly);

            var autoWiringApi = typeof(LifestyleAttribute).Assembly;

            return new DependencyContainerImpl(AssembliesExtensions.AllFromCurrentDomain(),
                                           new[] { autoWiringApi });
        }
    }
}