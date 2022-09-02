namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SimpleInjector;

    [Component(EnLifestyle.Singleton)]
    internal class ContainerImplementationConfigurationVerifier : IConfigurationVerifier,
                                                                  ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly Container _container;

        public ContainerImplementationConfigurationVerifier(Container container)
        {
            _container = container;
        }

        public void Verify()
        {
            _container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}