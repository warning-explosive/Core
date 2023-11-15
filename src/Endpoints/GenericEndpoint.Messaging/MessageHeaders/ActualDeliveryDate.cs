namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Actual delivery date to the input queue
    /// </summary>
    public record ActualDeliveryDate : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Actual delivery date</param>
        public ActualDeliveryDate(DateTime value)
        {
            Value = value;
        }

        /// <summary>
        /// Actual delivery date
        /// </summary>
        public DateTime Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.ToString("O");

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ActualDeliveryDate)}:{StringValue}]";
        }
    }
}