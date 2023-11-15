namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System.Globalization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Retry counter
    /// </summary>
    public record RetryCounter : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Retry counter</param>
        public RetryCounter(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Retry counter
        /// </summary>
        public int Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(RetryCounter)}:{StringValue}]";
        }
    }
}