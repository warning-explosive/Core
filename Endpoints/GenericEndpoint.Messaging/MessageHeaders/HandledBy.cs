namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;
    using Contract;

    /// <summary>
    /// HandledBy
    /// </summary>
    public record HandledBy : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public HandledBy()
        {
            Value = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="value">EndpointIdentity</param>
        public HandledBy(EndpointIdentity value)
        {
            Value = value;
        }

        /// <summary>
        /// Deferred until date
        /// </summary>
        public EndpointIdentity Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(HandledBy)}:{StringValue}]";
        }
    }
}