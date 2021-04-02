namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Abstraction for integration transport
    /// </summary>
    public interface IIntegrationTransport : IResolvable
    {
        /// <summary>
        /// OnMessage event
        /// Fires on receiving new incoming message
        /// </summary>
        event EventHandler<IntegrationMessageEventArgs> OnMessage;

        /// <summary>
        /// Gets ubiquitous integration context
        /// </summary>
        IUbiquitousIntegrationContext IntegrationContext { get; }
    }
}