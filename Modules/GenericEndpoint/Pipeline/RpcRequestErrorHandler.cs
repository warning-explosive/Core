namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Messaging;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using RpcRequest;

    [Component(EnLifestyle.Singleton)]
    internal class RpcRequestErrorHandler : IErrorHandler,
                                            ICollectionResolvable<IErrorHandler>
    {
        private readonly IRpcRequestRegistry _rpcRequestRegistry;
        private readonly ILogger _logger;

        public RpcRequestErrorHandler(
            IRpcRequestRegistry rpcRequestRegistry,
            ILogger logger)
        {
            _rpcRequestRegistry = rpcRequestRegistry;
            _logger = logger;
        }

        public Task Handle(
            IAdvancedIntegrationContext context,
            Exception exception,
            CancellationToken token)
        {
            if (context.Message.IsQuery())
            {
                var requestId = context.Message.ReadRequiredHeader<Id>().Value;

                if (_rpcRequestRegistry.TrySetException(requestId, exception))
                {
                    _logger.Warning($"Rpc request {requestId} was cancelled due to error: {exception}");
                }
            }

            return Task.CompletedTask;
        }
    }
}