namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Messaging;
    using UnitOfWork;

    [Component(EnLifestyle.Scoped)]
    internal class MessagesCollector : IMessagesCollector,
                                       IResolvable<IMessagesCollector>
    {
        private readonly ITransactionalOutbox _outbox;

        public MessagesCollector(ITransactionalOutbox outbox)
        {
            _outbox = outbox;
        }

        public void Collect(IntegrationMessage message)
        {
            _outbox.Add(message);
        }
    }
}