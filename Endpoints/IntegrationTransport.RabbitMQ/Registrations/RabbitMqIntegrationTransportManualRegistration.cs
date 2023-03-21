namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Registrations
{
    using Api.Abstractions;
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class RabbitMqIntegrationTransportManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IIntegrationTransport, RabbitMqIntegrationTransport>(EnLifestyle.Singleton);
            container.Register<IConfigurableIntegrationTransport, RabbitMqIntegrationTransport>(EnLifestyle.Singleton);
            container.Register<IExecutableIntegrationTransport, RabbitMqIntegrationTransport>(EnLifestyle.Singleton);
        }
    }
}