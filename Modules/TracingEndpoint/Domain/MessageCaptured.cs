namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using GenericDomain.Api.Abstractions;

    internal class MessageCaptured : IDomainEvent
    {
        public MessageCaptured(CapturedMessage capturedMessage)
        {
            CapturedMessage = capturedMessage;
        }

        public CapturedMessage CapturedMessage { get; }
    }
}