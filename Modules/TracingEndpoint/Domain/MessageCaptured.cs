namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using Contract;
    using GenericDomain.Api.Abstractions;

    internal class MessageCaptured : IDomainEvent
    {
        public MessageCaptured(
            SerializedIntegrationMessage serializedMessage,
            string? refuseReason)
        {
            SerializedMessage = serializedMessage;
            RefuseReason = refuseReason;
        }

        public SerializedIntegrationMessage SerializedMessage { get; }

        public string? RefuseReason { get; }
    }
}