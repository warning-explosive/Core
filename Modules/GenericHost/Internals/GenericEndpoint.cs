namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Async;
    using Core.GenericEndpoint.Abstractions;

    internal class GenericEndpoint : IRunnableEndpoint, IExecutableEndpoint, IGenericEndpoint
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly AsyncManualResetEvent _ready;
        private readonly AsyncCountdownEvent _runningHandlers;

        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer)
        {
            Identity = endpointIdentity;
            IntegrationTypesProvider = dependencyContainer.Resolve<IIntegrationTypesProvider>();

            _dependencyContainer = dependencyContainer;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IIntegrationTypesProvider IntegrationTypesProvider { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ValueTask DisposeAsync()
        {
            return new ValueTask(StopAsync());
        }

        public async Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IIntegrationContext context)
            where TMessage : IIntegrationMessage
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            _runningHandlers.Increment();
            using (Disposable.Create(() => _runningHandlers.Decrement()))
            {
                await _dependencyContainer
                    .Resolve<IMessageHandler<TMessage>>()
                    .Handle(message, context, Token)
                    .ConfigureAwait(false);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var initializer in _dependencyContainer.ResolveCollection<IEndpointInitializer>())
            {
                await initializer.Initialize(cancellationToken).ConfigureAwait(false);
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
                    .WhenAny(_runningHandlers.WaitAsync(Token), Task.Delay(Timeout.Infinite, Token))
                    .ConfigureAwait(false);
            }
        }
    }
}