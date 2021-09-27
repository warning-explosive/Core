namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericHost.Api.Abstractions;

    /// <summary>
    /// ITransportEndpointBuilder
    /// </summary>
    public interface ITransportEndpointBuilder
    {
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
        /// With default cross-cutting concerns
        /// </summary>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithDefaultCrossCuttingConcerns();

        /// <summary>
        /// With in-memory integration transport
        /// </summary>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithInMemoryIntegrationTransport();

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