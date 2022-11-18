﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Primitives;
    using Contract;
    using Microsoft.Extensions.Logging;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Extensions;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Enumerations;
    using UnitOfWork;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    internal class GenericEndpointDataAccessHostBackgroundWorker : IHostBackgroundWorker,
                                                                   ICollectionResolvable<IHostBackgroundWorker>,
                                                                   IResolvable<GenericEndpointDataAccessHostBackgroundWorker>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IExecutableIntegrationTransport _transport;
        private readonly ISettingsProvider<OutboxSettings> _outboxSettingProvider;
        private readonly IOutboxBackgroundDelivery _outboxDelivery;
        private readonly ILogger _logger;

        public GenericEndpointDataAccessHostBackgroundWorker(
            EndpointIdentity endpointIdentity,
            IExecutableIntegrationTransport transport,
            ISettingsProvider<OutboxSettings> outboxSettingProvider,
            IOutboxBackgroundDelivery outboxDelivery,
            ILogger logger)
        {
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _outboxSettingProvider = outboxSettingProvider;
            _outboxDelivery = outboxDelivery;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            var settings = await _outboxSettingProvider
               .Get(token)
               .ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(settings.OutboxDeliveryInterval, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                await ExecutionExtensions
                   .TryAsync((_transport, _outboxDelivery), DeliverMessages)
                   .Catch<Exception>(OnError(_logger))
                   .Invoke(token)
                   .ConfigureAwait(false);
            }
        }

        private static async Task DeliverMessages(
            (IExecutableIntegrationTransport, IOutboxBackgroundDelivery) state,
            CancellationToken token)
        {
            var (transport, outboxBackgroundDelivery) = state;

            var transportIsRunning = WaitUntilTransportIsRunning(transport, token);
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

        private static async Task WaitUntilTransportIsRunning(
            IExecutableIntegrationTransport transport,
            CancellationToken token)
        {
            using (var tcs = new TaskCancellationCompletionSource<object?>(token))
            {
                var subscription = MakeSubscription(tcs);

                using (Disposable.Create((transport, subscription), Subscribe, Unsubscribe))
                {
                    await tcs.Task.ConfigureAwait(false);
                }
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