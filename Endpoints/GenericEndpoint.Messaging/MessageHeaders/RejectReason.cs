namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;

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
        public string StringValue => Value.Message;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(RejectReason)}:{StringValue}]";
        }
    }
}