namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using IntegrationTransport.Api.Abstractions;
    using Mocks;

    internal class MessagesCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<MessagesCollector, MessagesCollector>(EnLifestyle.Singleton);
            container.RegisterDecorator<IIntegrationTransport, IntegrationTransportMessagesCollectorDecorator>(EnLifestyle.Singleton);
        }
    }
}