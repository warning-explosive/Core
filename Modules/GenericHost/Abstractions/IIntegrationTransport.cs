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
        /// OnError event
        /// Fires when message processing eventually fails after all retry attempts
        /// </summary>
        event EventHandler<FailedIntegrationMessageEventArgs> OnError;

        /// <summary>
        /// Gets ubiquitous integration context
        /// </summary>
        IUbiquitousIntegrationContext IntegrationContext { get; }
    }
}