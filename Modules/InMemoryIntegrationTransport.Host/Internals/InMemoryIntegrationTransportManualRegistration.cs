namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using Abstractions;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Defaults;
    using IntegrationTransport.Api.Abstractions;

    internal class InMemoryIntegrationTransportManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);
            container.Register<InMemoryIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);

            container.Register<IEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
            container.Register<DefaultEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);

            container.Register<IManualRegistration, InMemoryIntegrationTransportInjectionManualRegistration>(EnLifestyle.Singleton);
        }
    }
}