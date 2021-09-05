namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;
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
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(SentFrom)}] - [{Value}]";
        }
    }
}