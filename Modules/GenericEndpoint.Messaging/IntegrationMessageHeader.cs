namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    /// <summary>
    /// Integrated message headers
    /// </summary>
    public static class IntegrationMessageHeader
    {
        /// <summary>
        /// ConversationId - Guid, NotNull
        /// </summary>
        public const string ConversationId = "CONVERSATION_ID";

        /// <summary>
        /// Sent from - EndpointIdentity, CanBeNull
        /// </summary>
        public const string SentFrom = "SENT_FROM";

        /// <summary>
        /// Retry counter - Int32, CanBeNull
        /// </summary>
        public const string RetryCounter = "RETRY_COUNTER";

        /// <summary>
        /// Did handler replied to the query - Boolean, CanBeNull
        /// </summary>
        public const string DidHandlerReplyToTheQuery = "DID_HANDLER_REPLY_TO_THE_QUERY";

        /// <summary>
        /// Deferred until specified date (system time) - DateTime, CanBeNull
        /// </summary>
        public const string DeferredUntil = "DEFERRED_UNTIL";

        /// <summary>
        /// Actual delivery date to the input queue (system time) - DateTime, NotNull
        /// </summary>
        public const string ActualDeliveryDate = "ACTUAL_DELIVERY_DATE";
    }
}