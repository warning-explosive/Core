namespace SpaceEngineers.Core.CompositionRoot.SimpleInjector
{
    using System;
    using Api.Abstractions;
    using Internals;

    /// <summary>
    /// CompositionRoot extensions
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Use SimpleInjector DI container
        /// </summary>
        /// <param name="options">DependencyContainerOptions</param>
        /// <returns>Dependency container implementation producer</returns>
        // TODO: options.UseGenericContainer for tests - take implementation from configuration
        public static Func<IDependencyContainerImplementation> UseSimpleInjector(this DependencyContainerOptions options)
        {
            return () => new SimpleInjectorDependencyContainerImplementation();
        }
    }
}