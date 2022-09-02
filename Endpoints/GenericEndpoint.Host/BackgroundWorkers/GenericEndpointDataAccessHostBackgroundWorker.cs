namespace SpaceEngineers.Core.GenericEndpoint.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using DataAccess.UnitOfWork;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Api.Enumerations;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.CrossCuttingConcerns.Extensions;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.GenericEndpoint.DataAccess.Settings;

    internal class GenericEndpointDataAccessHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointDataAccessHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var settings = await _dependencyContainer
               .Resolve<ISettingsProvider<OutboxSettings>>()
               .Get(token)
               .ConfigureAwait(false);

            var endpointIdentity = _dependencyContainer.Resolve<Contract.EndpointIdentity>();
            var transport = _dependencyContainer.Resolve<IExecutableIntegrationTransport>();
            var outboxBackgroundDelivery = _dependencyContainer.Resolve<IOutboxBackgroundDelivery>();
            var logger = _dependencyContainer.Resolve<ILogger>();

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
                   .TryAsync((transport, outboxBackgroundDelivery), DeliverMessages)
                   .Catch<Exception>(OnError(endpointIdentity, logger))
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

        private static Func<Exception, CancellationToken, Task> OnError(
            Contract.EndpointIdentity endpointIdentity,
            ILogger logger)
        {
            return (exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Background outbox delivery error");
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