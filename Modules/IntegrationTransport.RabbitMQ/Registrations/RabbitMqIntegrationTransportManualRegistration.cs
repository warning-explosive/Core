namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Registrations
{
    using Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class RabbitMqIntegrationTransportManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IIntegrationTransport, RabbitMqIntegrationTransport>(EnLifestyle.Singleton);
        }
    }
}