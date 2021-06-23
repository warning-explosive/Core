namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Defaults;
    using IntegrationTransport.Api.Abstractions;

    internal class InMemoryIntegrationTransportManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IIntegrationTransport, InMemoryIntegrationTransport>();
            container.Register<InMemoryIntegrationTransport, InMemoryIntegrationTransport>();

            container.Register<IEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>();
            container.Register<DefaultEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>();

            container.Register<IManualRegistration, InMemoryIntegrationTransportInjectionManualRegistration>();
        }
    }
}