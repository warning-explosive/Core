namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Abstractions
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
        IReadOnlyCollection<string> ForAutomaticForwarding { get; }
    }
}