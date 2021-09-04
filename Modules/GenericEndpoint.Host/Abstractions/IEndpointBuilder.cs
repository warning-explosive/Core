namespace SpaceEngineers.Core.GenericEndpoint.Host.Abstractions
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using Contract;

    /// <summary>
    /// IEndpointBuilder
    /// </summary>
    public interface IEndpointBuilder
    {
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
        /// With data access
        /// </summary>
        /// <param name="databaseProvider">Database provider</param>
        /// <returns>EndpointBuilder</returns>
        public IEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider);

        /// <summary>
        /// Adds statistics pipeline into message processing in order to grab and store processing results
        /// </summary>
        /// <returns>IEndpointBuilder</returns>
        public IEndpointBuilder WithStatistics();

        /// <summary>
        /// Modify container options
        /// </summary>
        /// <param name="modifier">Modifier</param>
        /// <returns>IEndpointBuilder</returns>
        IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier);

        /// <summary>
        /// Build endpoint options
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>EndpointOptions</returns>
        EndpointOptions BuildOptions(EndpointIdentity endpointIdentity);
    }
}