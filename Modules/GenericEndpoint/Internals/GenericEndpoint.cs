namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using Contract;
    using Contract.Abstractions;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpoint : IRunnableEndpoint,
                                     IExecutableEndpoint
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly AsyncCountdownEvent _runningHandlers;
        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer,
            IEnumerable<IEndpointInitializer> initializers,
            IMessagePipeline messagePipeline)
        {
            Identity = endpointIdentity;
            DependencyContainer = dependencyContainer;
            Initializers = initializers;
            MessagePipeline = messagePipeline;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IDependencyContainer DependencyContainer { get; }

        public IEnumerable<IEndpointInitializer> Initializers { get; }

        public IMessagePipeline MessagePipeline { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task ProcessMessage(IntegrationMessage message)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            var handlerServiceType = typeof(IMessageHandler<>).MakeGenericType(message.ReflectedType);
            var messageHandlers = DependencyContainer.ResolveCollection(handlerServiceType);

            await InvokeMessageHandlers(messageHandlers, message)
                .WhenAll()
                .ConfigureAwait(false);
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var initializer in Initializers)
            {
                await initializer.Initialize(Token).ConfigureAwait(false);
            }

            _ready.Set();
        }

        public async Task StopAsync()
        {
            _ready.Reset();

            try
            {
                // Signal cancellation to the executing handlers
                _cts.Cancel();
            }
            finally
            {
                // Wait until completes all running handlers or the stop token triggers
                await Task
                    .WhenAny(_runningHandlers.WaitAsync(Token), Task.Delay(Timeout.InfiniteTimeSpan, Token))
                    .ConfigureAwait(false);
            }
        }

        private IEnumerable<Task> InvokeMessageHandlers(IEnumerable<object> messageHandlers, IntegrationMessage message)
        {
            foreach (var messageHandler in messageHandlers)
            {
                var copy = (IntegrationMessage)message.Clone();
                yield return InvokeMessageHandler(copy, messageHandler, Token);
            }
        }

        private async Task InvokeMessageHandler(IntegrationMessage message, object messageHandler, CancellationToken token)
        {
            var producer = InvokeMessageHandler(messageHandler);

            await using (DependencyContainer.OpenScopeAsync())
            {
                var exclusiveContext = DependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(message);

                using (Disposable.Create(_runningHandlers, e => e.Increment(), e => e.Decrement()))
                {
                    await MessagePipeline
                        .Process(producer, exclusiveContext, token)
                        .ConfigureAwait(false);
                }
            }
        }

        private static Func<IAdvancedIntegrationContext, CancellationToken, Task> InvokeMessageHandler(object messageHandler)
        {
            return (context, token) => messageHandler
                .CallMethod(nameof(IMessageHandler<IIntegrationMessage>.Handle))
                .WithArgument(context.Message.ReflectedType, context.Message.Payload)
                .WithArgument<IIntegrationContext>(context)
                .WithArgument(token)
                .Invoke<Task>();
        }
    }
}