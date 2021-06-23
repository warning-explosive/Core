namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Defaults;
    using IntegrationTransport.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransportInjectionManualRegistration : IManualRegistration
    {
        private readonly IDependencyContainer _dependencyContainer;

        public InMemoryIntegrationTransportInjectionManualRegistration(
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_dependencyContainer.Resolve<IIntegrationTransport>());
            container.RegisterInstance(_dependencyContainer.Resolve<InMemoryIntegrationTransport>());

            container.RegisterInstance(_dependencyContainer.Resolve<IEndpointInstanceSelectionBehavior>());
            container.RegisterInstance(_dependencyContainer.Resolve<DefaultEndpointInstanceSelectionBehavior>());
        }
    }
}