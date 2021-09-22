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

            PopulateEvent(new MessageCaptured(message, refuseReason));
        }

        public CapturedMessage(
            CapturedMessageDatabaseEntity captured,
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