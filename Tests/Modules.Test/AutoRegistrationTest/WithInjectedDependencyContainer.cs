namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;

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