namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainer : IWithInjectedDependencyContainer,
                                                     IResolvable<IWithInjectedDependencyContainer>
    {
        public WithInjectedDependencyContainer(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
        }

        public IDependencyContainer DependencyContainer { get; }
    }
}