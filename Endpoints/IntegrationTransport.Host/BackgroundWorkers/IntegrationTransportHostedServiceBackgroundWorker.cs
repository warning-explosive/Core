namespace SpaceEngineers.Core.IntegrationTransport.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using GenericHost;
    using Microsoft.Extensions.Logging;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    internal class IntegrationTransportHostedServiceBackgroundWorker : IHostedServiceBackgroundWorker,
                                                                       ICollectionResolvable<IHostedServiceObject>,
                                                                       ICollectionResolvable<IHostedServiceBackgroundWorker>,
                                                                       IResolvable<IntegrationTransportHostedServiceBackgroundWorker>
    {
        private readonly IExecutableIntegrationTransport _transport;
        private readonly ILogger _logger;

        public IntegrationTransportHostedServiceBackgroundWorker(
            IExecutableIntegrationTransport transport,
            ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            var subscription = MakeSubscription(_logger);

            using (Disposable.Create((_transport, subscription), Subscribe, Unsubscribe))
            {
                await _transport
                    .RunBackgroundMessageProcessing(token)
                    .ConfigureAwait(false);
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(
                ILogger logger)
            {
                return (sender, args) =>
                {
                    logger.Information($"{sender.GetType().Name} changed status from {args.PreviousStatus} to {args.CurrentStatus}");
                };
            }

            static void Subscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged += subscription;
            }

            static void Unsubscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged -= subscription;
            }
        }
    }
}