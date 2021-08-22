namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ContainerImplementationConfigurationVerifier : IConfigurationVerifier,
                                                                  ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly IDependencyContainerImplementation _container;

        public ContainerImplementationConfigurationVerifier(IDependencyContainerImplementation container)
        {
            _container = container;
        }

        public void Verify()
        {
            _container.Verify();
        }
    }
}