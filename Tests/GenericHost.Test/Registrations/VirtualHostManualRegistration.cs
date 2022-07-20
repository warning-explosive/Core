namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Settings;
    using IntegrationTransport.RabbitMQ.Settings;
    using Mocks;

    internal class VirtualHostManualRegistration : IManualRegistration
    {
        private readonly string _virtualHost;

        public VirtualHostManualRegistration(string virtualHost)
        {
            _virtualHost = virtualHost;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(new TestRabbitMqSettingsProviderDecorator.VirtualHostProvider(_virtualHost));
            container.RegisterDecorator<ISettingsProvider<RabbitMqSettings>, TestRabbitMqSettingsProviderDecorator>(EnLifestyle.Singleton);
        }
    }
}