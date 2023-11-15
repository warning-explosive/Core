namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using IntegrationTransport.Api.Abstractions;

    internal class IntegrationTransportManualRegistration : IManualRegistration
    {
        private readonly IIntegrationTransport _integrationTransport;

        public IntegrationTransportManualRegistration(IIntegrationTransport integrationTransport)
        {
            _integrationTransport = integrationTransport;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance((IIntegrationTransport)_integrationTransport);
            container.RegisterInstance((IConfigurableIntegrationTransport)_integrationTransport);
            container.RegisterInstance((IExecutableIntegrationTransport)_integrationTransport);
        }
    }
}