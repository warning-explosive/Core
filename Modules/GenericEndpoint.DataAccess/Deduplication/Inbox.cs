namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using Contract;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class Inbox : BaseAggregate
    {
        public Inbox(IntegrationMessage message, EndpointIdentity endpointIdentity)
        {
            Message = message;
            IsError = false;
            Handled = false;

            PopulateEvent(new InboxMessageReceived(message, endpointIdentity));
        }

        public Inbox(
            InboxMessageDatabaseEntity message,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            Message = message.Message.BuildIntegrationMessage(serializer, formatter);
            IsError = message.IsError;
            Handled = message.Handled;
        }

        public IntegrationMessage Message { get; }

        public bool IsError { get; private set; }

        public bool Handled { get; private set; }

        public void MarkAsHandled()
        {
            Handled = true;

            PopulateEvent(new InboxMessageWasHandled(Message.Id));
        }

        public void MarkAsError()
        {
            IsError = true;

            PopulateEvent(new InboxMessageWasMovedToErrorQueue(Message.Id));
        }
    }
}