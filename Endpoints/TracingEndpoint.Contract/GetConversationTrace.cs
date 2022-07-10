namespace SpaceEngineers.Core.TracingEndpoint.Contract
{
    using System;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

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