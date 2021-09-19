namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class Inbox : BaseAggregate
    {
        public Inbox(IntegrationMessage message)
        {
            Message = message;
            Handled = false;
            IsError = false;

            PopulateEvent(new InboxMessageReceived());
        }

        public Inbox(
            IntegrationMessageDatabaseEntity message,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            Message = message.BuildIntegrationMessage(serializer, formatter);
            Handled = message.Handled;
            IsError = message.IsError;
        }

        public IntegrationMessage Message { get; }

        public bool Handled { get; private set; }

        public bool IsError { get; private set; }

        public void MarkAsHandled()
        {
            Handled = true;
            PopulateEvent(new InboxMessageWasHandled());
        }

        public void MarkAsError()
        {
            IsError = true;
            PopulateEvent(new InboxMessageWasMovedToErrorQueue());
        }
    }
}