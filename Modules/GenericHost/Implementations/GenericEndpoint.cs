namespace SpaceEngineers.Core.GenericHost.Implementations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using Core.GenericEndpoint.Abstractions;
    using Internals;

    internal class GenericEndpoint : IExecutableEndpoint
    {
        private readonly IDependencyContainer _dependencyContainer;

        // TODO: use async counterpart
        private readonly ManualResetEventSlim _ready;
        private readonly ManualResetEventSlim _handlerIsRunning;

        private CancellationTokenSource? _cts;
        private int _runningHandlers;

        public GenericEndpoint(
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer)
        {
            Identity = endpointIdentity;
            IntegrationTypesProvider = dependencyContainer.Resolve<IIntegrationTypesProvider>();

            _dependencyContainer = dependencyContainer;

            _ready = new ManualResetEventSlim(false);
            _handlerIsRunning = new ManualResetEventSlim(false);
        }

        public EndpointIdentity Identity { get; }

        public IIntegrationTypesProvider IntegrationTypesProvider { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ValueTask DisposeAsync()
        {
            return new ValueTask(StopAsync());
        }

        public Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IIntegrationContext context)
            where TMessage : IIntegrationMessage
        {
            _ready.Wait(Token);

            var runningHandler = InvokeMessageHandlerWithTracking(message, context);

            return runningHandler;
        }

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var initializer in _dependencyContainer.ResolveCollection<IEndpointInitializer>())
            {
                await initializer.Initialize(cancellationToken).ConfigureAwait(false);
            }

            _ready.Set();
        }

        private async Task StopAsync()
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
                    .WhenAny(Task.Factory.StartNew(() => _handlerIsRunning.Wait(Token), Token), Task.Delay(Timeout.Infinite, Token))
                    .ConfigureAwait(false);
            }
        }

        private async Task InvokeMessageHandlerWithTracking<TMessage>(TMessage message, IIntegrationContext context)
            where TMessage : IIntegrationMessage
        {
            // TODO: recode with async counter
            _handlerIsRunning.Reset();
            Interlocked.Increment(ref _runningHandlers);

            await _dependencyContainer
                .Resolve<IMessageHandler<TMessage>>()
                .Handle(message, context, Token)
                .ConfigureAwait(false);

            var actual = Interlocked.Decrement(ref _runningHandlers);

            if (actual <= 0)
            {
                _handlerIsRunning.Set();
            }
        }
    }
}