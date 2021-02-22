namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Abstraction for integration transport
    /// </summary>
    public interface IIntegrationTransport : IResolvable
    {
        /// <summary>
        /// OnMessage event
        /// Fires on receiving new incoming message
        /// </summary>
        event EventHandler<IntegrationMessageEventArgs> OnMessage;

        /// <summary>
        /// Dependency container
        /// </summary>
        IDependencyContainer DependencyContainer { get; }

        /// <summary>
        /// Initialize transport (topology, state, etc...)
        /// Invokes multiple times for each in-process endpoint
        /// </summary>
        /// <param name="endpoints">Generic endpoints</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing initialization operation</returns>
        Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken token);

        /// <summary>
        /// IIntegrationContext factory
        /// </summary>
        /// <returns>IIntegrationContext</returns>
        IIntegrationContext CreateContext();

        /// <summary>
        /// Dispatch incoming message to endpoint
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Running operation</returns>
        Task DispatchToEndpoint(IntegrationMessage message);
    }
}