namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    internal interface IRpcReplyMessageHandler<TReply> : IResolvable
        where TReply : IIntegrationReply
    {
        Task Handle(IntegrationMessage message);
    }
}