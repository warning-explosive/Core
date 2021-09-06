namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using IntegrationTransport.Api.Abstractions;
    using Mocks;

    internal class MessagesCollectorInstanceManualRegistration : IManualRegistration
    {
        private readonly MessagesCollector _collector;

        public MessagesCollectorInstanceManualRegistration(MessagesCollector collector)
        {
            _collector = collector;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterDecorator<IIntegrationTransport, IntegrationTransportMessagesCollectorDecorator>(EnLifestyle.Singleton);
            container.RegisterInstance(_collector);
        }
    }
}