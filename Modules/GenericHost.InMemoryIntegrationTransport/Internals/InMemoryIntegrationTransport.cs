namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Async;
    using Core.GenericHost;
    using Core.GenericHost.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Registrations;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransport : IAdvancedIntegrationTransport
    {
        private readonly AsyncManualResetEvent _manualResetEvent;

        public InMemoryIntegrationTransport(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
            Injection = new EndpointInjectionRegistration(this);

            _manualResetEvent = new AsyncManualResetEvent(false);
        }

        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        public IManualRegistration Injection { get; }

        public IDependencyContainer DependencyContainer { get; }

        public IUbiquitousIntegrationContext IntegrationContext => DependencyContainer.Resolve<IUbiquitousIntegrationContext>();

        public Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken token)
        {
            _manualResetEvent.Set();

            return Task.CompletedTask;
        }

        internal async Task NotifyOnMessage(IntegrationMessage message, CancellationToken token)
        {
            await _manualResetEvent.WaitAsync(token).ConfigureAwait(false);

            OnMessage?.Invoke(this, new IntegrationMessageEventArgs(message));
        }
    }
}