namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
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