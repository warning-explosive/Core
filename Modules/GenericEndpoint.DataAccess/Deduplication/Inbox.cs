namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    internal class Inbox : BaseAggregate
    {
        public Inbox(IntegrationMessage message, EndpointIdentity endpointIdentity)
        {
            Message = message;
            IsError = false;
            Handled = false;

            PopulateEvent(new InboxMessageReceived(Id, message, endpointIdentity));
        }

        public Inbox(
            InboxMessage message,
            IJsonSerializer serializer)
        {
            Id = message.PrimaryKey;
            Message = message.Message.BuildIntegrationMessage(serializer);
            IsError = message.IsError;
            Handled = message.Handled;
        }

        public IntegrationMessage Message { get; }

        public bool IsError { get; private set; }

        public bool Handled { get; private set; }

        public void MarkAsHandled()
        {
            Handled = true;

            PopulateEvent(new InboxMessageWasHandled(Id));
        }

        public void MarkAsError()
        {
            IsError = true;

            PopulateEvent(new InboxMessageWasMovedToErrorQueue(Id));
        }
    }
}