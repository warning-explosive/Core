namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System.Globalization;
    using GenericEndpoint.Messaging.MessageHeaders;

    /// <summary>
    /// DeliveryTag
    /// </summary>
    public class DeliveryTag : IIntegrationMessageHeader
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
        public ulong Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(DeliveryTag)}:{StringValue}]";
        }
    }
}