namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Integration types provider
    /// </summary>
    public interface IIntegrationTypesProvider : IResolvable
    {
        /// <summary>
        /// Receive endpoint integration commands
        /// </summary>
        /// <returns>Endpoint integration commands</returns>
        IEnumerable<Type> EndpointCommands();

        /// <summary>
        /// Receive endpoint integration queries
        /// </summary>
        /// <returns>Endpoint integration queries</returns>
        IEnumerable<Type> EndpointQueries();

        /// <summary>
        /// Receive endpoint integration events
        /// </summary>
        /// <returns>Endpoint integration events</returns>
        IEnumerable<Type> EndpointEvents();

        /// <summary>
        /// Receive endpoint subscriptions to external integration events
        /// Events which have IMessageHandler implementation
        /// </summary>
        /// <returns>Endpoint integration subscriptions</returns>
        IEnumerable<Type> EndpointSubscriptions();
    }
}