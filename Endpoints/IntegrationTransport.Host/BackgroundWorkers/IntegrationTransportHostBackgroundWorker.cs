namespace SpaceEngineers.Core.IntegrationTransport.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using CompositionRoot;
    using CrossCuttingConcerns.Extensions;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    internal class IntegrationTransportHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public IntegrationTransportHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var transport = _dependencyContainer.Resolve<IExecutableIntegrationTransport>();
            var logger = _dependencyContainer.Resolve<ILogger>();

            transport.StatusChanged += OnStatusChanged(logger);

            await transport
                .StartBackgroundMessageProcessing(token)
                .ConfigureAwait(false);
        }

        private static EventHandler<IntegrationTransportStatusChangedEventArgs> OnStatusChanged(ILogger logger)
        {
            return (sender, args) => logger.Information($"{sender.GetType().Name}: {args.PreviousStatus} -> {args.CurrentStatus}");
        }
    }
}