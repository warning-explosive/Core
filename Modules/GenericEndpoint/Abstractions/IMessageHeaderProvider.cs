namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IMessageHeaderProvider abstraction
    /// </summary>
    public interface IMessageHeaderProvider : ICollectionResolvable<IMessageHeaderProvider>
    {
        /// <summary>
        /// Returns headers for automatic forwarding
        /// </summary>
        IEnumerable<string> ForAutomaticForwarding { get; }
    }
}