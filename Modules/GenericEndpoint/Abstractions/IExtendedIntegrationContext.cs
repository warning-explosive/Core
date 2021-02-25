namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;
    using Basics;
    using GenericEndpoint;

    /// <summary>
    /// IExtendedIntegrationContext abstraction
    /// </summary>
    public interface IExtendedIntegrationContext : IIntegrationContext, IInitializable<EndpointRuntimeInfo>
    {
        /// <summary>
        /// Integration message, processing initiator
        /// </summary>
        IntegrationMessage Message { get; }

        /// <summary>
        /// Current endpoint identity
        /// </summary>
        EndpointIdentity EndpointIdentity { get; }

        /// <summary>
        /// Retry integration message processing
        /// Must be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="dueTime">Time that transport waits before deliver message again</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing retry operation</returns>
        Task Retry(TimeSpan dueTime, CancellationToken token);

        /// <summary>
        /// Subscribe current integration context to logical transaction callbacks
        /// </summary>
        /// <param name="unitOfWorkBuilder">Unit of work builder</param>
        /// <returns>Opened context scope</returns>
        IAsyncDisposable WithinEndpointScope(AsyncUnitOfWorkBuilder<EndpointIdentity> unitOfWorkBuilder);
    }
}