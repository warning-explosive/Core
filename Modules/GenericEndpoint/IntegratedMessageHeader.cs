namespace SpaceEngineers.Core.GenericEndpoint
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    /// <summary>
    /// Integrated message headers
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public class IntegratedMessageHeader : IMessageHeaderProvider
    {
        /// <summary>
        /// ConversationId - guid, notnull
        /// </summary>
        public const string ConversationId = "CONVERSATION_ID";

        /// <summary>
        /// Sent from - EndpointIdentity, notnull
        /// </summary>
        public const string SentFrom = "SENT_FROM";

        /// <summary>
        /// Message retry counter - integer, notnull
        /// </summary>
        public const string MessageRetryCounter = "MESSAGE_RETRY_COUNTER";

        /// <summary>
        /// Handler replied to the query - boolean, notnull
        /// </summary>
        public const string HandlerRepliedToTheQuery = "HANDLER_REPLIED_TO_THE_QUERY";

        /// <inheritdoc />
        public IEnumerable<string> ForAutomaticForwarding { get; }
            = new[]
            {
                ConversationId,
                MessageRetryCounter
            };
    }
}