namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Integration types provider
    /// </summary>
    public interface IIntegrationTypeProvider
    {
        /// <summary>
        /// All types that implements IIntegrationMessage directly or indirectly and referenced to endpoint
        /// </summary>
        /// <returns>All IIntegrationMessage types</returns>
        IReadOnlyCollection<Type> IntegrationMessageTypes();

        /// <summary>
        /// Receive endpoint integration commands
        /// Commands that could be handled by endpoint (owned by this endpoint and have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration commands</returns>
        IReadOnlyCollection<Type> EndpointCommands();

        /// <summary>
        /// Receive endpoint integration queries
        /// Queries that could be handled by endpoint (owned by this endpoint and have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration queries</returns>
        IReadOnlyCollection<Type> EndpointQueries();

        /// <summary>
        /// Receive endpoint subscriptions to integration replies
        /// Replies that could be handled by endpoint (have IMessageHandler implementation)
        /// </summary>
        /// <returns>All integration replies</returns>
        IReadOnlyCollection<Type> RepliesSubscriptions();

        /// <summary>
        /// Receive endpoint subscriptions to integration events
        /// Events that could be handled by endpoint (have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration subscriptions</returns>
        IReadOnlyCollection<Type> EventsSubscriptions();
    }
}