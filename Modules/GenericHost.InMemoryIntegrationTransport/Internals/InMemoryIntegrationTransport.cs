namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Primitives;
    using Core.GenericHost;
    using Core.GenericHost.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Registrations;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransport : IAdvancedIntegrationTransport
    {
        private readonly AsyncManualResetEvent _ready;

        public InMemoryIntegrationTransport(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
            Injection = new InMemoryTransportInjectionRegistration(this);

            _ready = new AsyncManualResetEvent(false);
        }

        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        public event EventHandler<FailedIntegrationMessageEventArgs>? OnError;

        public IManualRegistration Injection { get; }

        public IDependencyContainer DependencyContainer { get; }

        public IUbiquitousIntegrationContext IntegrationContext => DependencyContainer.Resolve<IUbiquitousIntegrationContext>();

        public Task Initialize(IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>> endpoints, CancellationToken token)
        {
            _ready.Set();

            return Task.CompletedTask;
        }

        internal async Task NotifyOnMessage(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            OnMessage?.Invoke(this, new IntegrationMessageEventArgs(message));
        }

        internal async Task NotifyOnError(FailedMessage failedMessage, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            OnError?.Invoke(this, new FailedIntegrationMessageEventArgs(failedMessage));
        }
    }
}