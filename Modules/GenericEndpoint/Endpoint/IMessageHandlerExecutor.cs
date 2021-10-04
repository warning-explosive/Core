namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Contract.Abstractions;
    using Messaging;

    internal interface IMessageHandlerExecutor<TMessage> : IResolvable
        where TMessage : IIntegrationMessage
    {
        Task Invoke(IntegrationMessage message, CancellationToken token);
    }
}