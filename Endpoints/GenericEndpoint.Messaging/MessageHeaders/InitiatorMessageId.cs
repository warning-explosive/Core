namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;

    /// <summary>
    /// Initiator message id
    /// </summary>
    public class InitiatorMessageId : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Initiator message id</param>
        public InitiatorMessageId(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Initiator message id
        /// </summary>
        public Guid Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(InitiatorMessageId)}:{StringValue}]";
        }
    }
}