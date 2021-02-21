namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// ICompositeEndpoint abstraction
    /// </summary>
    public interface ICompositeEndpoint
    {
        /// <summary>
        /// Endpoints
        /// </summary>
        IReadOnlyCollection<IGenericEndpoint> Endpoints { get; }
    }
}