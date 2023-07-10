namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System.Globalization;
    using System.Text.Json.Serialization;
    using GenericEndpoint.Messaging.MessageHeaders;

    /// <summary>
    /// DeliveryTag
    /// </summary>
    public record DeliveryTag : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Delivery tag value</param>
        public DeliveryTag(ulong value)
        {
            Value = value;
        }

        /// <summary>
        /// Delivery tag value
        /// </summary>
        public ulong Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(DeliveryTag)}:{StringValue}]";
        }
    }
}