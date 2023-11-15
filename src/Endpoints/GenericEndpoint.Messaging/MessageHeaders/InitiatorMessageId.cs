namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Initiator message id
    /// </summary>
    public record InitiatorMessageId : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Initiator message id</param>
        public InitiatorMessageId(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Initiator message id
        /// </summary>
        public Guid Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(InitiatorMessageId)}:{StringValue}]";
        }
    }
}