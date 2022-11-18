namespace SpaceEngineers.Core.IntegrationTransport.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Extensions;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    internal class IntegrationTransportHostBackgroundWorker : IHostBackgroundWorker,
                                                              ICollectionResolvable<IHostBackgroundWorker>,
                                                              IResolvable<IntegrationTransportHostBackgroundWorker>
    {
        private readonly IExecutableIntegrationTransport _transport;
        private readonly ILogger _logger;

        public IntegrationTransportHostBackgroundWorker(
            IExecutableIntegrationTransport transport,
            ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            _transport.StatusChanged += OnStatusChanged(_logger);

            await _transport
                .StartBackgroundMessageProcessing(token)
                .ConfigureAwait(false);
        }

        private static EventHandler<IntegrationTransportStatusChangedEventArgs> OnStatusChanged(ILogger logger)
        {
            return (sender, args) => logger.Information($"{sender.GetType().Name} changed status from {args.PreviousStatus} to {args.CurrentStatus}");
        }
    }
}