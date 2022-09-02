namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Contract.Abstractions;
    using Messaging;
    using Pipeline;

    [Component(EnLifestyle.Transient)]
    internal class MessageHandlerExecutor<TMessage> : IMessageHandlerExecutor<TMessage>,
                                                      IResolvable<IMessageHandlerExecutor<TMessage>>
        where TMessage : IIntegrationMessage
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IMessagePipeline _messagePipeline;

        public MessageHandlerExecutor(
            IDependencyContainer dependencyContainer,
            IMessagePipeline messagePipeline)
        {
            _dependencyContainer = dependencyContainer;
            _messagePipeline = messagePipeline;
        }

        public async Task Invoke(IntegrationMessage message, CancellationToken token)
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(message);
                var messageHandler = _dependencyContainer.Resolve<IMessageHandler<TMessage>>();

                await _messagePipeline.Process(
                        HandleProducer((TMessage)message.Payload, messageHandler),
                        exclusiveContext,
                        token)
                   .ConfigureAwait(false);
            }
        }

        private static Func<IAdvancedIntegrationContext, CancellationToken, Task> HandleProducer(
            TMessage message,
            IMessageHandler<TMessage> messageHandler)
        {
            return (_, token) => messageHandler.Handle(message, token);
        }
    }
}