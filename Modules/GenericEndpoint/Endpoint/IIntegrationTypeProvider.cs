namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
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
        IEnumerable<Type> IntegrationMessageTypes();

        /// <summary>
        /// Receive endpoint integration commands
        /// Commands that could be handled by endpoint (owned by this endpoint and have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration commands</returns>
        IEnumerable<Type> EndpointCommands();

        /// <summary>
        /// Receive endpoint integration queries
        /// Queries that could be handled by endpoint (owned by this endpoint and have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration queries</returns>
        IEnumerable<Type> EndpointQueries();

        /// <summary>
        /// Receive endpoint subscriptions to integration replies
        /// Replies that could be handled by endpoint (have IMessageHandler implementation)
        /// </summary>
        /// <returns>All integration replies</returns>
        IEnumerable<Type> RepliesSubscriptions();

        /// <summary>
        /// Receive endpoint subscriptions to integration events
        /// Events that could be handled by endpoint (have IMessageHandler implementation)
        /// </summary>
        /// <returns>Endpoint integration subscriptions</returns>
        IEnumerable<Type> EventsSubscriptions();
    }
}