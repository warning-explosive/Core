namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Messaging;

    internal class CapturedMessage : BaseAggregate
    {
        public CapturedMessage(
            IntegrationMessage message,
            string? refuseReason)
        {
            Message = message;
            RefuseReason = refuseReason;

            PopulateEvent(new MessageCaptured(this));
        }

        public CapturedMessage(
            IntegrationMessageDatabaseEntity message,
            string? refuseReason,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            Message = message.BuildIntegrationMessage(serializer, formatter);
            RefuseReason = refuseReason;
        }

        public IntegrationMessage Message { get; }

        public string? RefuseReason { get; }
    }
}