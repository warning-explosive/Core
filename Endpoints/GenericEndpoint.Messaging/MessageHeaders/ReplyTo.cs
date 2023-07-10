namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;
    using Contract;

    /// <summary>
    /// Reply to specified endpoint
    /// </summary>
    public record ReplyTo : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public ReplyTo()
        {
            Value = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="value">Initiator endpoint identity</param>
        public ReplyTo(EndpointIdentity value)
        {
            Value = value;
        }

        /// <summary>
        /// Initiator endpoint identity
        /// </summary>
        public EndpointIdentity Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ReplyTo)}:{StringValue}]";
        }
    }
}