namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
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
        }

        public IntegrationMessage Message { get; }

        public string? RefuseReason { get; }
    }
}