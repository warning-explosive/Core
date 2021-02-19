namespace SpaceEngineers.Core.GenericHost.Endpoint
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Basics.Async;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;
    using InternalAbstractions;

    [ManualRegistration]
    internal class GenericEndpoint : IGenericEndpoint, IRunnableEndpoint, IExecutableEndpoint, IMessagePipeline
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly AsyncManualResetEvent _ready;
        private readonly AsyncCountdownEvent _runningHandlers;

        private CancellationTokenSource? _cts;

        public GenericEndpoint(EndpointOptions endpointOptions)
        {
            Identity = endpointOptions.Identity;

            _dependencyContainer = DependencyContainerPerEndpoint(endpointOptions);
            IntegrationTypesProvider = _dependencyContainer.Resolve<IIntegrationTypesProvider>();

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

        public Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IExtendedIntegrationContext context)
            where TMessage : IIntegrationMessage
        {
            return Process(message, context, Token);
        }

        public async Task Process<TMessage>(TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            _runningHandlers.Increment();
            using (Disposable.Create(_runningHandlers, @event => @event.Decrement()))
            {
                await _dependencyContainer
                    .Resolve<IMessageHandler<TMessage>>()
                    .Handle(message, context, Token)
                    .ConfigureAwait(false);
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var initializer in _dependencyContainer.ResolveCollection<IEndpointInitializer>())
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

        private IDependencyContainer DependencyContainerPerEndpoint(EndpointOptions endpointOptions)
        {
            var containerOptions = endpointOptions.ContainerOptions ?? new DependencyContainerOptions();

            var registrations = new List<IManualRegistration>(containerOptions.ManualRegistrations)
            {
                new EndpointManualRegistration(this)
            };

            containerOptions.ManualRegistrations = registrations;

            return endpointOptions.Assembly != null
                ? DependencyContainer.CreateBoundedAbove(endpointOptions.Assembly, containerOptions)
                : DependencyContainer.Create(containerOptions);
        }

        private class EndpointManualRegistration : IManualRegistration
        {
            private readonly GenericEndpoint _endpoint;

            public EndpointManualRegistration(GenericEndpoint endpoint)
            {
                _endpoint = endpoint;
            }

            public void Register(IRegistrationContainer container)
            {
                container.Register<IMessagePipeline>(() => _endpoint, EnLifestyle.Singleton);
                container.Register<GenericEndpoint>(() => _endpoint, EnLifestyle.Singleton);
                container.Register<EndpointIdentity>(() => _endpoint.Identity, EnLifestyle.Singleton);
            }
        }
    }
}