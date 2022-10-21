namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm.Host.Abstractions;

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
        /// Build endpoint options
        /// </summary>
        /// <returns>EndpointOptions</returns>
        EndpointOptions BuildOptions();
    }
}