namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
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
            container.RegisterInstance(_collector);
        }
    }
}