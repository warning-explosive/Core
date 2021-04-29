namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Registrations
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Core.GenericHost.Abstractions;
    using GenericEndpoint.Abstractions;
    using Internals;

    internal class InMemoryTransportInjectionRegistration : IManualRegistration
    {
        private readonly InMemoryIntegrationTransport _transport;

        public InMemoryTransportInjectionRegistration(InMemoryIntegrationTransport transport)
        {
            _transport = transport;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<IIntegrationTransport>(_transport);
            container.RegisterInstance<InMemoryIntegrationTransport>(_transport);

            container.Register<IAdvancedIntegrationContext, InMemoryIntegrationContext>();
            container.Register<InMemoryIntegrationContext, InMemoryIntegrationContext>();

            container.Register<IIntegrationMessageFactory, IntegrationMessageFactory>();
            container.Register<IntegrationMessageFactory, IntegrationMessageFactory>();
        }
    }
}