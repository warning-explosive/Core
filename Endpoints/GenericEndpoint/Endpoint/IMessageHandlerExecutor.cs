namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contract.Abstractions;
    using Messaging;

    internal interface IMessageHandlerExecutor<TMessage>
        where TMessage : IIntegrationMessage
    {
        Task Invoke(IntegrationMessage message, CancellationToken token);
    }
}