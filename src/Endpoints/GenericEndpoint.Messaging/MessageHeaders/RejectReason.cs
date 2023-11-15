namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Reject reason
    /// </summary>
    public record RejectReason : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public RejectReason()
        {
            Value = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="value">Reject reason</param>
        public RejectReason(Exception value)
        {
            Value = value;
        }

        /// <summary>
        /// Reject reason
        /// </summary>
        public Exception Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.Message;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(RejectReason)}:{StringValue}]";
        }
    }
}