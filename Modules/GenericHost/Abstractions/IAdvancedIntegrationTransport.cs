namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Abstraction for integration transport with additional features
    /// </summary>
    public interface IAdvancedIntegrationTransport : IIntegrationTransport
    {
        /// <summary>
        /// Manual registration with transport dependencies for endpoints
        /// </summary>
        IManualRegistration Injection { get; }

        /// <summary>
        /// Dependency container
        /// </summary>
        IDependencyContainer DependencyContainer { get; }

        /// <summary>
        /// Initialize transport (topology, state, subscriptions, callbacks, etc...)
        /// </summary>
        /// <param name="endpoints">Generic endpoints</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing initialization operation</returns>
        Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken token);
    }
}