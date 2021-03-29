namespace SpaceEngineers.Core.GenericEndpoint.Executable.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Async;
    using Contract.Abstractions;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpoint : IGenericEndpoint,
                                     IRunnableEndpoint,
                                     IExecutableEndpoint,
                                     IMessagePipeline
    {
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
            IntegrationTypeProvider = integrationTypeProvider;

            DependencyContainer = dependencyContainer;
            Initializers = initializers;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new AsyncCountdownEvent(0);
        }

        public EndpointIdentity Identity { get; }

        public IIntegrationTypeProvider IntegrationTypeProvider { get; }

        private IDependencyContainer DependencyContainer { get; }

        private IEnumerable<IEndpointInitializer> Initializers { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task InvokeMessageHandler(IntegrationMessage message)
        {
            await using (DependencyContainer.OpenScopeAsync())
            {
                var exclusiveContext = DependencyContainer.Resolve<IExtendedIntegrationContext, IntegrationMessage>(message);

                await DependencyContainer
                    .Resolve<IMessagePipeline>()
                    .Process(exclusiveContext, Token)
                    .ConfigureAwait(false);
            }
        }

        public async Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            using (Disposable.Create(_runningHandlers, e => e.Increment(), e => e.Decrement()))
            {
                var handlerServiceType = typeof(IMessageHandler<>).MakeGenericType(context.Message.ReflectedType);

                await DependencyContainer
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
                    .WhenAny(_runningHandlers.WaitAsync(Token), Task.Delay(Timeout.Infinite, Token))
                    .ConfigureAwait(false);
            }
        }
    }
}