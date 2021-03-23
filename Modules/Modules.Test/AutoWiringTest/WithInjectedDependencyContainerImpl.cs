namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainerImpl : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainerImpl(IDependencyContainer dependencyContainer,
                                                   IVersionedContainer versionedContainer,
                                                   IScopedContainer scopedContainer)
        {
            DependencyContainer = dependencyContainer;
            VersionedContainer = versionedContainer;
            ScopedContainer = scopedContainer;
        }

        public IDependencyContainer DependencyContainer { get; }

        public IVersionedContainer VersionedContainer { get; }

        public IScopedContainer ScopedContainer { get; }
    }
}