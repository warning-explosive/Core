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
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Singleton)]
    internal class ErrorHandlingRpcReplyMessageHandler<TReply> : IRpcReplyMessageHandler<TReply>,
                                                                 IDecorator<IRpcReplyMessageHandler<TReply>>
        where TReply : IIntegrationReply
    {
        private readonly IIntegrationTransport _transport;
        private readonly ILogger _logger;

        public ErrorHandlingRpcReplyMessageHandler(
            IRpcReplyMessageHandler<TReply> decoratee,
            IIntegrationTransport transport,
            ILogger logger)
        {
            Decoratee = decoratee;
            _transport = transport;
            _logger = logger;
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
            return (exception, token) =>
            {
                _logger.Error(exception);
                return _transport.EnqueueError(message, exception, token);
            };
        }
    }
}