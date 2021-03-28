namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Registrations
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Core.GenericHost.Abstractions;
    using GenericEndpoint.Abstractions;
    using Internals;

    internal class EndpointInjectionRegistration : IManualRegistration
    {
        private readonly InMemoryIntegrationTransport _transport;

        public EndpointInjectionRegistration(InMemoryIntegrationTransport transport)
        {
            _transport = transport;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<IIntegrationTransport>(_transport);
            container.RegisterInstance<InMemoryIntegrationTransport>(_transport);

            container.Register<IExtendedIntegrationContext, InMemoryIntegrationContext>();
            container.Register<InMemoryIntegrationContext, InMemoryIntegrationContext>();

            container.Register<IIntegrationMessageFactory, IntegrationMessageFactory>();
            container.Register<IntegrationMessageFactory, IntegrationMessageFactory>();
        }
    }
}