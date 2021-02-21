namespace SpaceEngineers.Core.GenericHost.Internals
{
    using AutoWiringApi.Abstractions;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Contract.Abstractions;

    internal interface IIntegrationMessageFactory : IResolvable
    {
        IntegrationMessage CreateGeneralMessage<TMessage>(
            TMessage messageToSend,
            EndpointIdentity? endpointIdentity,
            IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage;
    }
}