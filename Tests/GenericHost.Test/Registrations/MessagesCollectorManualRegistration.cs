namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using CompositionRoot.Registration;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class MessagesCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<TestMessagesCollector, TestMessagesCollector>(EnLifestyle.Scoped);
        }
    }
}