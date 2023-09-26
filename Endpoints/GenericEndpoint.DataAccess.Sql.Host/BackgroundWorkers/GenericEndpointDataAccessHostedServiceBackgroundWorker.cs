namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using GenericEndpoint.Host.StartupActions;
    using GenericHost;
    using Microsoft.Extensions.Logging;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Enumerations;
    using UnitOfWork;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [After(typeof(GenericEndpointHostedServiceStartupAction))]
    internal class GenericEndpointDataAccessHostedServiceBackgroundWorker : IHostedServiceBackgroundWorker,
                                                                            ICollectionResolvable<IHostedServiceObject>,
                                                                            ICollectionResolvable<IHostedServiceBackgroundWorker>,
                                                                            IResolvable<GenericEndpointDataAccessHostedServiceBackgroundWorker>
    {
        private readonly OutboxSettings _outboxSetting;
        private readonly IExecutableIntegrationTransport _transport;
        private readonly IOutboxBackgroundDelivery _outboxDelivery;
        private readonly ILogger _logger;

        public GenericEndpointDataAccessHostedServiceBackgroundWorker(
            ISettingsProvider<OutboxSettings> outboxSettingProvider,
            IExecutableIntegrationTransport transport,
            IOutboxBackgroundDelivery outboxDelivery,
            ILogger logger)
        {
            _outboxSetting = outboxSettingProvider.Get();

            _transport = transport;
            _outboxDelivery = outboxDelivery;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_outboxSetting.OutboxDeliveryInterval, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                await DeliverMessages(_transport, _outboxDelivery, token)
                   .TryAsync()
                   .Catch<Exception>(OnError(_logger))
                   .Invoke(token)
                   .ConfigureAwait(false);
            }
        }

        private static async Task DeliverMessages(
            IExecutableIntegrationTransport transport,
            IOutboxBackgroundDelivery outboxBackgroundDelivery,
            CancellationToken token)
        {
            var transportIsRunning = WaitUntilTransportIsRunning(transport);
            var outboxDelivery = outboxBackgroundDelivery.DeliverMessages(token);

            try
            {
                await Task
                   .WhenAny(transportIsRunning, outboxDelivery)
                   .Unwrap()
                   .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static Func<Exception, CancellationToken, Task> OnError(ILogger logger)
        {
            return (exception, _) =>
            {
                logger.Error(exception, "Background outbox delivery error");
                return Task.CompletedTask;
            };
        }

        private static async Task WaitUntilTransportIsRunning(IExecutableIntegrationTransport transport)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = MakeSubscription(tcs);

            using (Disposable.Create((transport, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(
                TaskCompletionSource<object?> tcs)
            {
                return (_, args) =>
                {
                    if (args.CurrentStatus != EnIntegrationTransportStatus.Running)
                    {
                        _ = tcs.TrySetResult(default);
                    }
                };
            }

            static void Subscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (transport, subscription) = state;
                transport.StatusChanged += subscription;
            }

            static void Unsubscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (transport, subscription) = state;
                transport.StatusChanged -= subscription;
            }
        }
    }
}