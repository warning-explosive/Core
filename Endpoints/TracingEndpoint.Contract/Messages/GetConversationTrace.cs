namespace SpaceEngineers.Core.TracingEndpoint.Contract.Messages
{
    using System;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// GetConversationTrace
    /// </summary>
    [OwnedBy(TracingEndpointIdentity.LogicalName)]
    public class GetConversationTrace : IIntegrationQuery<ConversationTrace>
    {
        /// <summary> .cctor </summary>
        /// <param name="conversationId">Conversation id</param>
        public GetConversationTrace(Guid conversationId)
        {
            ConversationId = conversationId;
        }

        /// <summary>
        /// Conversation id
        /// </summary>
        public Guid ConversationId { get; init; }
    }
}