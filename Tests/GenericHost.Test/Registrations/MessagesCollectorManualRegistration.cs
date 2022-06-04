namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

    internal class MessagesCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<MessagesCollector, MessagesCollector>(EnLifestyle.Singleton);
            container.RegisterDecorator<IIntegrationTransport, IntegrationTransportMessagesCollectorDecorator>(EnLifestyle.Singleton);
        }
    }
}