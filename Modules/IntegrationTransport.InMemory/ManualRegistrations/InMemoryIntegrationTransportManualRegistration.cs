namespace SpaceEngineers.Core.IntegrationTransport.InMemory.ManualRegistrations
{
    using Api.Abstractions;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class InMemoryIntegrationTransportManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);
            container.Register<InMemoryIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);

            container.Register<IEndpointInstanceSelectionBehavior, EndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
            container.Register<EndpointInstanceSelectionBehavior, EndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);

            container.Register<IManualRegistration, InMemoryIntegrationTransportInjectionManualRegistration>(EnLifestyle.Singleton);
        }
    }
}