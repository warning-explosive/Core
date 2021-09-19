namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Messaging;

    internal interface IMessageHandlerExecutor<TMessage> : IResolvable
    {
        Task Invoke(IntegrationMessage message, CancellationToken token);
    }
}