namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiringApi.Abstractions;
    using Contract.Abstractions;
    using GenericEndpoint;

    /// <summary>
    /// IIntegrationMessageFactory abstraction
    /// </summary>
    public interface IIntegrationMessageFactory : IResolvable
    {
        /// <summary>
        /// Creates IntegrationMessage instance from user defined payload
        /// </summary>
        /// <param name="payload">User defined payload message</param>
        /// <param name="endpointIdentity">Optional endpoint identity</param>
        /// <param name="initiatorMessage">Optional initiator message</param>
        /// <typeparam name="TMessage">IntegrationMessage type-argument</typeparam>
        /// <returns>IntegrationMessage instance</returns>
        IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointIdentity? endpointIdentity, IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage;
    }
}