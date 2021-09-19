namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot.Api.Abstractions.Container;
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
            IEnumerable<IEndpointInitializer> initializers)
        {
            Identity = endpointIdentity;
            DependencyContainer = dependencyContainer;
            Initializers = initializers;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IDependencyContainer DependencyContainer { get; }

        public IEnumerable<IEndpointInitializer> Initializers { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

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

        public async Task ExecuteMessageHandlers(IntegrationMessage message)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            var handlerServiceType = typeof(IMessageHandlerExecutor<>).MakeGenericType(message.ReflectedType);
            var executor = DependencyContainer.Resolve(handlerServiceType);

            using (Disposable.Create(_runningHandlers, e => e.Increment(), e => e.Decrement()))
            {
                await executor
                    .CallMethod(nameof(IMessageHandlerExecutor<IIntegrationMessage>.Invoke))
                    .WithArguments(message, _cts.Token)
                    .Invoke<Task>()
                    .ConfigureAwait(false);
            }
        }
    }
}