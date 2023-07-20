namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using Mocks;

    internal class MessagesCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<TestMessagesCollector, TestMessagesCollector>(EnLifestyle.Scoped);
        }
    }
}