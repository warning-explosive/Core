namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using Abstractions;

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
        object IIntegrationMessageHeader.Value => Value;
    }
}