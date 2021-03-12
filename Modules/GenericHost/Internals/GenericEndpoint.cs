namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using Basics;
    using Basics.Async;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;
    using Core.GenericEndpoint.Contract.Abstractions;

    [ManualRegistration]
    internal class GenericEndpoint : IGenericEndpoint, IRunnableEndpoint, IExecutableEndpoint, IMessagePipeline
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IEnumerable<IEndpointInitializer> _initializers;

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
            _dependencyContainer = dependencyContainer;
            IntegrationTypeProvider = integrationTypeProvider;

            _initializers = initializers;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IIntegrationTypeProvider IntegrationTypeProvider { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
        }

        public async Task InvokeMessageHandler(IntegrationMessage message)
        {
            await using (_dependencyContainer.OpenScopeAsync())
            {
                var exclusiveContext = _dependencyContainer.Resolve<IExtendedIntegrationContext, IntegrationMessage>(message);

                await _dependencyContainer
                    .Resolve<IMessagePipeline>()
                    .Process(exclusiveContext, Token)
                    .ConfigureAwait(false);
            }
        }

        public async Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            _runningHandlers.Increment();
            using (Disposable.Create(_runningHandlers, e => e.Decrement()))
            {
                var handlerServiceType = typeof(IMessageHandler<>).MakeGenericType(context.Message.ReflectedType);

                await _dependencyContainer
                    .Resolve(handlerServiceType)
                    .CallMethod(nameof(IMessageHandler<IIntegrationMessage>.Handle))
                    .WithArguments(context.Message.Payload, context, Token)
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