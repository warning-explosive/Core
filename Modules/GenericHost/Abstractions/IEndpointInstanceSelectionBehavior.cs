namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System.Collections.Generic;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// IEndpointInstanceSelectionBehavior
    /// </summary>
    public interface IEndpointInstanceSelectionBehavior
    {
        /// <summary>
        /// Select instance in one logical group
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="endpoints">Endpoint instances</param>
        /// <returns>Only one instance</returns>
        IGenericEndpoint SelectInstance(
            IIntegrationMessage message,
            IReadOnlyCollection<IGenericEndpoint> endpoints);
    }
}