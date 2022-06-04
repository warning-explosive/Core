namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class ErrorHandlingRpcReplyMessageHandler<TReply> : IRpcReplyMessageHandler<TReply>,
                                                                 IDecorator<IRpcReplyMessageHandler<TReply>>
        where TReply : IIntegrationReply
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTransport _transport;

        public ErrorHandlingRpcReplyMessageHandler(
            IRpcReplyMessageHandler<TReply> decoratee,
            EndpointIdentity endpointIdentity,
            IIntegrationTransport transport)
        {
            Decoratee = decoratee;
            _endpointIdentity = endpointIdentity;
            _transport = transport;
        }

        public IRpcReplyMessageHandler<TReply> Decoratee { get; }

        public Task Handle(IntegrationMessage message, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(message, Decoratee.Handle)
                .Catch<Exception>(OnError(message))
                .Invoke(token);
        }

        private Func<Exception, CancellationToken, Task> OnError(IntegrationMessage message)
        {
            return (exception, token) => _transport.EnqueueError(_endpointIdentity, message, exception, token);
        }
    }
}