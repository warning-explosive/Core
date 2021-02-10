namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Abstraction for integration transport
    /// </summary>
    public interface IIntegrationTransport
    {
        /// <summary>
        /// OnMessage event
        /// Fires on receiving new incoming message
        /// </summary>
        event EventHandler<IntegrationMessageEventArgs> OnMessage;

        /// <summary>
        /// Initialize transport (topology, state, etc...)
        /// Invokes multiple times for each in-process endpoint
        /// </summary>
        /// <param name="endpoints">Generic endpoints</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ongoing initialization operation</returns>
        Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken cancellationToken);

        /// <summary>
        /// IIntegrationContext factory
        /// </summary>
        /// <returns>IIntegrationContext</returns>
        IIntegrationContext CreateContext();

        /// <summary>
        /// Dispatch incoming message to endpoint
        /// </summary>
        /// <param name="message">IIntegrationMessage</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Running operation</returns>
        Task DispatchToEndpoint<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage;
    }
}