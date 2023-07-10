namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// ConversationId
    /// </summary>
    public record ConversationId : IIntegrationMessageHeader
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
        public Guid Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ConversationId)}:{StringValue}]";
        }
    }
}