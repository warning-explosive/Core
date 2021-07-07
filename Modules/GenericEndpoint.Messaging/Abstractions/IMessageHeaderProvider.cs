namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// IMessageHeaderProvider abstraction
    /// </summary>
    public interface IMessageHeaderProvider
    {
        /// <summary>
        /// Returns headers for automatic forwarding
        /// </summary>
        IReadOnlyCollection<string> ForAutomaticForwarding { get; }
    }
}