namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;

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
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(DeliveryTag)}] - [{Value}]";
        }
    }
}