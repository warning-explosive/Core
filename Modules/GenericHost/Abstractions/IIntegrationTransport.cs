namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using System.Threading.Tasks;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Abstraction for integration transport
    /// </summary>
    public interface IIntegrationTransport
    {
        /// <summary>
        /// OnMessage event
        /// </summary>
        event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        /// <summary>
        /// IIntegrationContext getter
        /// </summary>
        /// <returns>IIntegrationContext</returns>
        IIntegrationContext Context { get; }

        /// <summary>
        /// Initialize transport topology
        /// Invokes multiple times for each in-process endpoint
        /// </summary>
        /// <param name="endpoint">IGenericEndpoint</param>
        /// <returns>Ongoing initialization operation</returns>
        Task InitializeTopology(IGenericEndpoint endpoint);

        /// <summary>
        /// Dispatch incoming message to endpoint
        /// </summary>
        /// <param name="message">IIntegrationMessage</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Running operation</returns>
        Task Dispatch<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage;
    }
}