namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
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