namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;

    /// <summary>
    /// DidHandlerReplyToTheQuery
    /// </summary>
    public class DidHandlerReplyToTheQuery : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Did handler reply to the query</param>
        public DidHandlerReplyToTheQuery(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// Value
        /// </summary>
        public bool Value { get; }

        /// <inheritdoc />
        object IIntegrationMessageHeader.Value => Value;
    }
}