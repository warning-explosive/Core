namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;

    /// <summary>
    /// ConversationId
    /// </summary>
    public class ConversationId : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Conversation id</param>
        public ConversationId(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Value
        /// </summary>
        public Guid Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ConversationId)}:{StringValue}]";
        }
    }
}