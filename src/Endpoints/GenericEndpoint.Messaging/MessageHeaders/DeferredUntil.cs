namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Deferred until date defers delivery of a message until it arrives
    /// The message will be delivered no earlier than this date and not exactly in that time due to asynchronous nature of the messaging
    /// </summary>
    public record DeferredUntil : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Deferred until date</param>
        public DeferredUntil(DateTime value)
        {
            Value = value;
        }

        /// <summary>
        /// Deferred until date
        /// </summary>
        public DateTime Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString("O");

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(DeferredUntil)}:{StringValue}]";
        }
    }
}