namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using Abstractions;

    /// <summary>
    /// Reject reason
    /// </summary>
    public class RejectReason : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Reject reason</param>
        public RejectReason(Exception value)
        {
            Value = value;
        }

        /// <summary>
        /// Reject reason
        /// </summary>
        public Exception Value { get; }

        /// <inheritdoc />
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(RejectReason)}] - [{Value.Message}]";
        }
    }
}