namespace SpaceEngineers.Core.CompositionRoot.SimpleInjector
{
    using System;
    using Api.Abstractions.Container;
    using Internals;

    /// <summary>
    /// CompositionRoot extensions
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Use SimpleInjector as DI container
        /// </summary>
        /// <param name="options">DependencyContainerOptions</param>
        /// <returns>Dependency container implementation producer</returns>
        public static Func<IDependencyContainerImplementation> UseSimpleInjector(this DependencyContainerOptions options)
        {
            return () => new SimpleInjectorDependencyContainerImplementation();
        }
    }
}