namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contract.Abstractions;
    using GenericEndpoint;

    /// <summary>
    /// IExtendedIntegrationContext abstraction
    /// </summary>
    public interface IExtendedIntegrationContext : IIntegrationContext
    {
        /// <summary>
        /// Retry integration message processing
        /// Must be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Ongoing retry operation</returns>
        Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage;

        /// <summary>
        /// Apply endpoint scope to current integration context
        /// </summary>
        /// <param name="endpointScope">Endpoint scope</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Scope resource</returns>
        IAsyncDisposable WithinEndpointScope(EndpointScope endpointScope, CancellationToken token);
    }
}