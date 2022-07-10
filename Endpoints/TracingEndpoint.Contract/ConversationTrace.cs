namespace SpaceEngineers.Core.TracingEndpoint.Contract
{
    using System;
    using System.Collections.Generic;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// ConversationTrace
    /// </summary>
    public class ConversationTrace : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="correlationId">Conversation id</param>
        /// <param name="serializedMessage">Serialized message</param>
        /// <param name="refuseReason">Refuse reason</param>
        /// <param name="subsequentTrace">Subsequent trace</param>
        public ConversationTrace(
            Guid correlationId,
            SerializedIntegrationMessage? serializedMessage = null,
            string? refuseReason = null,
            IReadOnlyCollection<ConversationTrace>? subsequentTrace = null)
        {
            ConversationId = correlationId;
            SerializedMessage = serializedMessage;
            RefuseReason = refuseReason;
            SubsequentTrace = subsequentTrace ?? Array.Empty<ConversationTrace>();
        }

        /// <summary>
        /// Conversation id
        /// </summary>
        public Guid ConversationId { get; init; }

        /// <summary>
        /// Serialized message
        /// </summary>
        public SerializedIntegrationMessage? SerializedMessage { get; init; }

        /// <summary>
        /// Refuse reason
        /// </summary>
        public string? RefuseReason { get; init; }

        /// <summary>
        /// Subsequent trace
        /// </summary>
        public IReadOnlyCollection<ConversationTrace> SubsequentTrace { get; init; }
    }
}