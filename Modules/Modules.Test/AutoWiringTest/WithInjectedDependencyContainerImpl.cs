namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainerImpl : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainerImpl(IDependencyContainer dependencyContainer,
                                                   IScopedContainer scopedContainer)
        {
            DependencyContainer = dependencyContainer;
            ScopedContainer = scopedContainer;
        }

        public IDependencyContainer DependencyContainer { get; }

        public IScopedContainer ScopedContainer { get; }
    }
}