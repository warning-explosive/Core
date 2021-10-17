namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Api.Abstractions;
    using IntegrationMessage = GenericEndpoint.Messaging.IntegrationMessage;

    internal class CapturedMessage : BaseAggregate
    {
        public CapturedMessage(
            IntegrationMessage message,
            string? refuseReason)
        {
            Message = message;
            RefuseReason = refuseReason;

            PopulateEvent(new MessageCaptured(message, refuseReason));
        }

        public CapturedMessage(
            DatabaseModel.CapturedMessage captured,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            Message = captured.Message.BuildIntegrationMessage(serializer, formatter);
            RefuseReason = captured.RefuseReason;
        }

        public IntegrationMessage Message { get; }

        public string? RefuseReason { get; }
    }
}