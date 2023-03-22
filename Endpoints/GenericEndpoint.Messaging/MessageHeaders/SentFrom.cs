namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Contract;

    /// <summary>
    /// Initiator endpoint identity
    /// </summary>
    public class SentFrom : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Initiator endpoint identity</param>
        public SentFrom(EndpointIdentity value)
        {
            Value = value;
        }

        /// <summary>
        /// Initiator endpoint identity
        /// </summary>
        public EndpointIdentity Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(SentFrom)}:{StringValue}]";
        }
    }
}