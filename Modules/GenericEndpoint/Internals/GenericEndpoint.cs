namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Primitives;
    using Contract;
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

        public async Task InvokeMessageHandler(IntegrationMessage message)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            await using (DependencyContainer.OpenScopeAsync())
            {
                var exclusiveContext = DependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>((IntegrationMessage)message.Clone());

                using (Disposable.Create(_runningHandlers, e => e.Increment(), e => e.Decrement()))
                {
                    await DependencyContainer
                        .Resolve<IMessagePipeline>()
                        .Process(exclusiveContext, Token)
                        .ConfigureAwait(false);
                }
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var initializer in Initializers)
            {
                await initializer.Initialize(token).ConfigureAwait(false);
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
    }
}