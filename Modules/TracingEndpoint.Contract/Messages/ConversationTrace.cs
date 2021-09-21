namespace SpaceEngineers.Core.TracingEndpoint.Contract.Messages
{
    using System;
    using System.Collections.Generic;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// ConversationTrace
    /// </summary>
    public class ConversationTrace : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="correlationId">Conversation id</param>
        /// <param name="message">Integration message</param>
        /// <param name="refuseReason">Refuse reason</param>
        /// <param name="subsequentTrace">Subsequent trace</param>
        public ConversationTrace(
            Guid correlationId,
            IntegrationMessage message,
            string? refuseReason = null,
            IReadOnlyCollection<ConversationTrace>? subsequentTrace = null)
        {
            ConversationId = correlationId;
            Message = message;
            RefuseReason = refuseReason;
            SubsequentTrace = subsequentTrace ?? Array.Empty<ConversationTrace>();
        }

        /// <summary> .cctor </summary>
        /// <param name="correlationId">Conversation id</param>
        public ConversationTrace(Guid correlationId)
        {
            ConversationId = correlationId;
        }

        /// <summary>
        /// Conversation id
        /// </summary>
        public Guid ConversationId { get; }

        /// <summary>
        /// Message
        /// </summary>
        public IntegrationMessage? Message { get; }

        /// <summary>
        /// Refuse reason
        /// </summary>
        public string? RefuseReason { get; }

        /// <summary>
        /// Subsequent trace
        /// </summary>
        public IReadOnlyCollection<ConversationTrace>? SubsequentTrace { get; }
    }
}