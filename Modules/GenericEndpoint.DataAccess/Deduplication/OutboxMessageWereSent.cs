namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class OutboxMessageWereSent : IDomainEvent
    {
        public OutboxMessageWereSent(IntegrationMessage message)
        {
            Message = message;
        }

        public IntegrationMessage Message { get; }
    }
}