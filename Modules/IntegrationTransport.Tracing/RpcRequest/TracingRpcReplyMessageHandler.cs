namespace SpaceEngineers.Core.IntegrationTransport.Tracing.RpcRequest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using IntegrationTransport.RpcRequest;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;

    [Component(EnLifestyle.Singleton)]
    [Dependent(typeof(ErrorHandlingRpcReplyMessageHandler<>))]
    internal class TracingRpcReplyMessageHandler<TReply> : IRpcReplyMessageHandler<TReply>,
                                                           IDecorator<IRpcReplyMessageHandler<TReply>>
        where TReply : IIntegrationReply
    {
        private readonly IIntegrationContext _context;
        private readonly IJsonSerializer _jsonSerializer;

        public TracingRpcReplyMessageHandler(
            IRpcReplyMessageHandler<TReply> decoratee,
            IIntegrationContext context,
            IJsonSerializer jsonSerializer)
        {
            Decoratee = decoratee;

            _context = context;
            _jsonSerializer = jsonSerializer;
        }

        public IRpcReplyMessageHandler<TReply> Decoratee { get; }

        public Task Handle(IntegrationMessage message, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(message, HandleInternal)
                .Catch<Exception>(OnError(message))
                .Invoke(token);
        }

        private async Task HandleInternal(IntegrationMessage message, CancellationToken token)
        {
            await Decoratee.Handle(message, token).ConfigureAwait(false);
            await OnSuccess(message, token).ConfigureAwait(false);
        }

        private Task OnSuccess(IntegrationMessage message, CancellationToken token)
        {
            var command = new CaptureTrace(SerializedIntegrationMessage.FromIntegrationMessage(message, _jsonSerializer), null);
            return _context.Send(command, token);
        }

        private Func<Exception, CancellationToken, Task> OnError(IntegrationMessage message)
        {
            return async (exception, token) =>
            {
                var command = new CaptureTrace(SerializedIntegrationMessage.FromIntegrationMessage(message, _jsonSerializer), exception);
                await _context.Send(command, token).ConfigureAwait(false);

                throw exception.Rethrow();
            };
        }
    }
}