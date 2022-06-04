namespace SpaceEngineers.Core.IntegrationTransport.Host.ManualRegistrations
{
    using Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class IntegrationTransportInjectionManualRegistration : IManualRegistration
    {
        private readonly IDependencyContainer _dependencyContainer;

        public IntegrationTransportInjectionManualRegistration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate<IIntegrationTransport>(() => _dependencyContainer.Resolve<IIntegrationTransport>(), EnLifestyle.Singleton);
        }
    }
}