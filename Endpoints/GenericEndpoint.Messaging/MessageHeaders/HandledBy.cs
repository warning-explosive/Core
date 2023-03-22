namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Contract;

    /// <summary>
    /// HandledBy
    /// </summary>
    public class HandledBy : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">EndpointIdentity</param>
        public HandledBy(EndpointIdentity value)
        {
            Value = value;
        }

        /// <summary>
        /// Deferred until date
        /// </summary>
        public EndpointIdentity Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(HandledBy)}:{StringValue}]";
        }
    }
}