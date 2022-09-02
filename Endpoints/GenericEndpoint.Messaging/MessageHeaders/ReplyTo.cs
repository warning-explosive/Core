namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Contract;

    /// <summary>
    /// Reply to specified endpoint
    /// </summary>
    public class ReplyTo : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Initiator endpoint identity</param>
        public ReplyTo(EndpointIdentity value)
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
            return $"[{nameof(ReplyTo)}] - [{Value}]";
        }
    }
}