namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainer : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainer(IDependencyContainer dependencyContainer,
                                               IScopedContainer scopedContainer)
        {
            DependencyContainer = dependencyContainer;
            ScopedContainer = scopedContainer;
        }

        public IDependencyContainer DependencyContainer { get; }

        public IScopedContainer ScopedContainer { get; }
    }
}