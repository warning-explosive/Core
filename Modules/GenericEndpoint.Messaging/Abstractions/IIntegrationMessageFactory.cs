namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Abstractions
{
    using Contract;
    using Contract.Abstractions;

    /// <summary>
    /// IIntegrationMessageFactory abstraction
    /// </summary>
    public interface IIntegrationMessageFactory
    {
        /// <summary>
        /// Creates IntegrationMessage instance from user defined payload
        /// </summary>
        /// <param name="payload">User defined payload message</param>
        /// <param name="endpointIdentity">Optional EndpointIdentity</param>
        /// <param name="initiatorMessage">Optional initiator message</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>IntegrationMessage instance</returns>
        IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointIdentity? endpointIdentity, IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage;
    }
}