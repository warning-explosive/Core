namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainerImpl : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainerImpl(IDependencyContainer dependencyContainer,
                                                   IVersionedContainer versionedContainer,
                                                   IRegistrationContainer registrationContainer,
                                                   IScopedContainer scopedContainer)
        {
            DependencyContainer = dependencyContainer;
            VersionedContainer = versionedContainer;
            RegistrationContainer = registrationContainer;
            ScopedContainer = scopedContainer;
        }

        public IDependencyContainer DependencyContainer { get; }

        public IVersionedContainer VersionedContainer { get; }

        public IRegistrationContainer RegistrationContainer { get; }

        public IScopedContainer ScopedContainer { get; }
    }
}