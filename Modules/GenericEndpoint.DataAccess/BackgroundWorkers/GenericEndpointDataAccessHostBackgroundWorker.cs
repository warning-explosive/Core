namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot.Api.Abstractions;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Extensions;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using DatabaseModel;
    using GenericEndpoint.UnitOfWork;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Api.Enumerations;
    using Microsoft.Extensions.Logging;
    using Settings;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

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

            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();
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
                   .TryAsync(_dependencyContainer, DeliverMessages)
                   .Catch<Exception>(OnError(endpointIdentity, logger))
                   .Invoke(token)
                   .ConfigureAwait(false);
            }
        }

        private static async Task DeliverMessages(
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            var transport = dependencyContainer.Resolve<IIntegrationTransport>();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var transportIsRunning = WaitUntilTransportIsRunning(transport, cts.Token);
                var outboxDelivery = DeliverMessagesUnsafe(dependencyContainer, cts.Token);

                await Task
                   .WhenAny(transportIsRunning, outboxDelivery)
                   .ConfigureAwait(false);
            }
        }

        private static async Task DeliverMessagesUnsafe(
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            var endpointIdentity = dependencyContainer.Resolve<EndpointIdentity>();
            var jsonSerializer = dependencyContainer.Resolve<IJsonSerializer>();

            var messages = await dependencyContainer
               .InvokeWithinTransaction(true,
                    (endpointIdentity, jsonSerializer),
                    ReadMessages,
                    token)
               .ConfigureAwait(false);

            await using (dependencyContainer.OpenScopeAsync())
            {
                await dependencyContainer
                   .Resolve<IOutboxDelivery>()
                   .DeliverMessages(messages, token)
                   .ConfigureAwait(false);
            }

            static async Task<IReadOnlyCollection<IntegrationMessage>> ReadMessages(
                IDatabaseTransaction transaction,
                (EndpointIdentity, IJsonSerializer) state,
                CancellationToken token)
            {
                var (endpointIdentity, serializer) = state;

                return (await transaction
                       .Read<OutboxMessage, Guid>()
                       .All()
                       .Where(outbox => outbox.EndpointIdentity.LogicalName == endpointIdentity.LogicalName
                                     && !outbox.Sent)
                       .Select(outbox => outbox.Message)
                       .ToListAsync(token)
                       .ConfigureAwait(false))
                   .Select(message => message.BuildIntegrationMessage(serializer))
                   .ToList();
            }
        }

        private static Func<Exception, CancellationToken, Task> OnError(
            EndpointIdentity endpointIdentity,
            ILogger logger)
        {
            return (exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Background outbox delivery error");
                return Task.CompletedTask;
            };
        }

        private static async Task WaitUntilTransportIsRunning(
            IIntegrationTransport transport,
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
                        tcs.SetResult(default);
                    }
                };
            }

            static void Subscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (transport, subscription) = state;
                transport.StatusChanged += subscription;
            }

            static void Unsubscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (transport, subscription) = state;
                transport.StatusChanged -= subscription;
            }
        }
    }
}