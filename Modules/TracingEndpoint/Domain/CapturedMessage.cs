namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using Contract;
    using GenericDomain.Api.Abstractions;

    internal class CapturedMessage : BaseAggregate
    {
        public CapturedMessage(
            SerializedIntegrationMessage serializedMessage,
            string? refuseReason)
        {
            SerializedMessage = serializedMessage;
            RefuseReason = refuseReason;

            PopulateEvent(new MessageCaptured(serializedMessage, refuseReason));
        }

        public CapturedMessage(DatabaseModel.CapturedMessage captured)
        {
            SerializedMessage = captured.Message.BuildSerializedIntegrationMessage();
            RefuseReason = captured.RefuseReason;
        }

        public SerializedIntegrationMessage SerializedMessage { get; }

        public string? RefuseReason { get; }
    }
}