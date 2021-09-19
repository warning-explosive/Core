namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract.Abstractions;
    using Messaging;
    using Pipeline;

    [Component(EnLifestyle.Transient)]
    internal class MessageHandlerExecutor<TMessage> : IMessageHandlerExecutor<TMessage>
        where TMessage : IIntegrationMessage
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IEnumerable<IMessageHandler<TMessage>> _messageHandlers;
        private readonly IMessagePipeline _messagePipeline;

        public MessageHandlerExecutor(
            IDependencyContainer dependencyContainer,
            IEnumerable<IMessageHandler<TMessage>> messageHandlers,
            IMessagePipeline messagePipeline)
        {
            _dependencyContainer = dependencyContainer;
            _messageHandlers = messageHandlers;
            _messagePipeline = messagePipeline;
        }

        public async Task Invoke(IntegrationMessage message, CancellationToken token)
        {
            using (var enumerator = _messageHandlers.GetEnumerator())
            {
                while (true)
                {
                    await using (_dependencyContainer.OpenScopeAsync())
                    {
                        if (enumerator.MoveNext())
                        {
                            var copy = message.Clone();
                            await InvokeScopedHandler(copy, enumerator.Current, token).ConfigureAwait(false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private async Task InvokeScopedHandler(
            IntegrationMessage message,
            IMessageHandler<TMessage> messageHandler,
            CancellationToken token)
        {
            var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(message);

            await _messagePipeline
                .Process(HandleProducer((TMessage)message.Payload, messageHandler), exclusiveContext, token)
                .ConfigureAwait(false);
        }

        private static Func<IAdvancedIntegrationContext, CancellationToken, Task> HandleProducer(
            TMessage message,
            IMessageHandler<TMessage> messageHandler)
        {
            return (context, token) => messageHandler.Handle(message, context, token);
        }
    }
}