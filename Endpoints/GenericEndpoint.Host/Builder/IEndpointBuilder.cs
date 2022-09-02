namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm.Connection;
    using GenericHost.Api.Abstractions;

    /// <summary>
    /// IEndpointBuilder
    /// </summary>
    public interface IEndpointBuilder
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
        /// With endpoint plugin assemblies
        /// </summary>
        /// <param name="assemblies">Endpoint plugin assemblies</param>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// With data access
        /// </summary>
        /// <param name="databaseProvider">Database provider</param>
        /// <param name="dataAccessOptions">Data access options</param>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithDataAccess(
            IDatabaseProvider databaseProvider,
            Action<DataAccessOptions>? dataAccessOptions);

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
        /// <returns>EndpointOptions</returns>
        EndpointOptions BuildOptions();
    }
}