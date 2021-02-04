namespace SpaceEngineers.Core.GenericHost.Hosts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using GenericEndpoint.Abstractions;
    using Internals;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class GenericEndpointHostedService : IHostedService,
                                                  IGenericEndpoint
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ILogger<GenericEndpointHostedService> _logger;

        private bool _ready;
        private CancellationTokenSource? _stoppingCts;

        public GenericEndpointHostedService(
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer,
            ILogger<GenericEndpointHostedService> logger)
        {
            Identity = endpointIdentity;
            IntegrationTypesProvider = dependencyContainer.Resolve<IIntegrationTypesProvider>();

            _dependencyContainer = dependencyContainer;
            _logger = logger;
        }

        public EndpointIdentity Identity { get; }

        public IIntegrationTypesProvider IntegrationTypesProvider { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var initializer in _dependencyContainer.ResolveCollection<IEndpointInitializer>())
            {
                await initializer.Initialize().ConfigureAwait(false);
            }

            _ready = true;
            _logger.Information(Resources.StartedSuccessfully, Identity);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingCts?.Cancel();
            return Task.CompletedTask;
        }

        public Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IIntegrationContext context)
            where TMessage : IIntegrationMessage
        {
            if (!_ready)
            {
                throw new InvalidOperationException("Endpoint isn't ready yet");
            }

            return _dependencyContainer
                .Resolve<IMessageHandler<TMessage>>()
                .Handle(message, context);
        }
    }
}