namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using Basics.Async;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;
    using Core.GenericEndpoint.Contract.Abstractions;

    [ManualRegistration]
    internal class GenericEndpoint : IGenericEndpoint, IRunnableEndpoint, IExecutableEndpoint, IMessagePipeline
    {
        private readonly IReadOnlyCollection<IEndpointInitializer> _initializers;

        private readonly AsyncManualResetEvent _ready;
        private readonly AsyncCountdownEvent _runningHandlers;

        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer,
            IIntegrationTypeProvider integrationTypeProvider,
            IEnumerable<IEndpointInitializer> initializers)
        {
            Identity = endpointIdentity;
            DependencyContainer = dependencyContainer;
            IntegrationTypeProvider = integrationTypeProvider;
            _initializers = initializers.ToList();

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IDependencyContainer DependencyContainer { get; }

        public IIntegrationTypeProvider IntegrationTypeProvider { get; }

        private IMessagePipeline Pipeline => DependencyContainer.Resolve<IMessagePipeline>();

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ValueTask DisposeAsync()
        {
            return new ValueTask(StopAsync());
        }

        public Task InvokeMessageHandler(IntegrationMessage message, IExtendedIntegrationContext context)
        {
            return Pipeline.Process(message, context, Token);
        }

        public async Task Process(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            _runningHandlers.Increment();
            using (Disposable.Create(_runningHandlers, @event => @event.Decrement()))
            {
                var handlerType = typeof(IMessageHandler<>).MakeGenericType(message.ReflectedType);

                await DependencyContainer
                    .Resolve(handlerType)
                    .CallMethod(nameof(IMessageHandler<IIntegrationMessage>.Handle))
                    .WithTypeArgument(message.ReflectedType)
                    .WithArguments(message.Payload, context, Token)
                    .Invoke<Task>()
                    .ConfigureAwait(false);
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var initializer in _initializers)
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
                    .WhenAny(_runningHandlers.WaitAsync(Token), Task.Delay(Timeout.Infinite, Token))
                    .ConfigureAwait(false);
            }
        }
    }
}