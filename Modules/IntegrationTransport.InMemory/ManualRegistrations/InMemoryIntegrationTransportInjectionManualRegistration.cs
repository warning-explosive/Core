namespace SpaceEngineers.Core.IntegrationTransport.InMemory.ManualRegistrations
{
    using Api.Abstractions;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class InMemoryIntegrationTransportInjectionManualRegistration : IManualRegistration
    {
        private readonly IDependencyContainer _dependencyContainer;

        public InMemoryIntegrationTransportInjectionManualRegistration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate<IIntegrationTransport>(() => _dependencyContainer.Resolve<IIntegrationTransport>(), EnLifestyle.Singleton);
            container.Advanced.RegisterDelegate<IEndpointInstanceSelectionBehavior>(() => _dependencyContainer.Resolve<IEndpointInstanceSelectionBehavior>(), EnLifestyle.Singleton);
        }
    }
}