namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using Abstractions;
    using CompositionRoot.Api.Abstractions;
    using Defaults;
    using IntegrationTransport.Api.Abstractions;

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