namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;
    using Core.DataAccess.Orm;
    using GenericHost.Api.Abstractions;

    /// <summary>
    /// IEndpointBuilder
    /// </summary>
    public interface IEndpointBuilder
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
        /// With endpoint plugin assemblies
        /// </summary>
        /// <param name="assemblies">Endpoint plugin assemblies</param>
        /// <returns>EndpointBuilder</returns>
        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// With default cross cutting concerns
        /// </summary>
        /// <returns>EndpointBuilder</returns>
        public IEndpointBuilder WithDefaultCrossCuttingConcerns();

        /// <summary>
        /// Adds tracing pipeline into message processing so as to collect and store message processing information
        /// </summary>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithTracing();

        /// <summary>
        /// With data access
        /// </summary>
        /// <param name="databaseProvider">Database provider</param>
        /// <returns>EndpointBuilder</returns>
        public IEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider);

        /// <summary>
        /// Modify container options
        /// </summary>
        /// <param name="modifier">Modifier</param>
        /// <returns>IEndpointBuilder</returns>
        IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier);

        /// <summary>
        /// Registers specified IHostStartupAction within generic host
        /// </summary>
        /// <param name="producer">IHostStartupAction producer</param>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer);

        /// <summary>
        /// Registers specified IHostBackgroundWorker within generic host
        /// </summary>
        /// <param name="producer">IHostBackgroundWorker producer</param>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer);

        /// <summary>
        /// Build endpoint options
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>EndpointOptions</returns>
        EndpointOptions BuildOptions(EndpointIdentity endpointIdentity);
    }
}