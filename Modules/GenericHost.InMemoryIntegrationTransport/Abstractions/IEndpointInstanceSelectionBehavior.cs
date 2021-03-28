namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Abstractions
{
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// IEndpointInstanceSelectionBehavior
    /// </summary>
    public interface IEndpointInstanceSelectionBehavior : IResolvable
    {
        /// <summary>
        /// Select instance in one logical group
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="endpoints">Endpoint instances</param>
        /// <returns>Only one instance</returns>
        IGenericEndpoint SelectInstance(
            IntegrationMessage message,
            IReadOnlyCollection<IGenericEndpoint> endpoints);
    }
}