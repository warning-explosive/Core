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
        /// <param name="endpointScope">Optional endpoint scope</param>
        /// <typeparam name="TMessage">IntegrationMessage type-argument</typeparam>
        /// <returns>IntegrationMessage instance</returns>
        IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointScope? endpointScope)
            where TMessage : IIntegrationMessage;
    }
}