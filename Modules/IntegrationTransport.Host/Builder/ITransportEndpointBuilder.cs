namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// ITransportEndpointBuilder
    /// </summary>
    public interface ITransportEndpointBuilder
    {
        /// <summary>
        /// Endpoint identity
        /// </summary>
        EndpointIdentity EndpointIdentity { get; }

        /// <summary>
        /// Host startup actions
        /// </summary>
        IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> StartupActions { get; }

        /// <summary>
        /// Host background workers
        /// </summary>
        IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> BackgroundWorkers { get; }

        /// <summary>
        /// With transport endpoint plugin assemblies
        /// </summary>
        /// <param name="assemblies">Transport endpoint plugin assemblies</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// Adds tracing pipeline into message processing so as to collect and store message processing information
        /// </summary>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithTracing();

        /// <summary>
        /// With in-memory integration transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithInMemoryIntegrationTransport(IHostBuilder hostBuilder);

        /// <summary>
        /// Modify container options
        /// </summary>
        /// <param name="modifier">Modifier</param>
        /// <returns>ITransportEndpointBuilder</returns>
        ITransportEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier);

        /// <summary>
        /// Registers specified IHostStartupAction within generic host
        /// </summary>
        /// <param name="producer">IHostStartupAction producer</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer);

        /// <summary>
        /// Registers specified IHostBackgroundWorker within generic host
        /// </summary>
        /// <param name="producer">IHostBackgroundWorker producer</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer);

        /// <summary>
        /// Build transport endpoint options
        /// </summary>
        /// <returns>TransportEndpointOptions</returns>
        TransportEndpointOptions BuildOptions();
    }
}