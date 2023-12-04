namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm.Sql.Linq;
    using Core.DataAccess.Orm.Sql.Transaction;
    using CrossCuttingConcerns.Logging;
    using Deduplication;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.UnitOfWork;
    using GenericHost;
    using Microsoft.Extensions.Logging;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Enumerations;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [After(typeof(GenericEndpointHostedServiceStartupAction))]
    internal class GenericEndpointDataAccessHostedServiceBackgroundWorker : IHostedServiceBackgroundWorker,
                                                                            ICollectionResolvable<IHostedServiceObject>,
                                                                            ICollectionResolvable<IHostedServiceBackgroundWorker>,
                                                                            IResolvable<GenericEndpointDataAccessHostedServiceBackgroundWorker>
    {
        private readonly OutboxSettings _outboxSettings;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IExecutableIntegrationTransport _transport;
        private readonly ILogger _logger;

        public GenericEndpointDataAccessHostedServiceBackgroundWorker(
            ISettingsProvider<OutboxSettings> outboxSettingsProvider,
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer,
            IExecutableIntegrationTransport transport,
            ILogger logger)
        {
            _outboxSettings = outboxSettingsProvider.Get();
            _endpointIdentity = endpointIdentity;
            _dependencyContainer = dependencyContainer;
            _transport = transport;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_outboxSettings.OutboxDeliveryInterval, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                await DeliverMessagesUnsafe(token)
                   .TryAsync()
                   .Catch<Exception>(OnError(_logger))
                   .Invoke(token)
                   .ConfigureAwait(false);
            }

            static Func<Exception, CancellationToken, Task> OnError(ILogger logger)
            {
                return (exception, _) =>
                {
                    logger.Error(exception, "Background outbox delivery error");
                    return Task.CompletedTask;
                };
            }
        }

        private async Task DeliverMessagesUnsafe(CancellationToken token)
        {
            if (((IIntegrationTransport)_transport).Status != EnIntegrationTransportStatus.Running)
            {
                return;
            }

            try
            {
                await _dependencyContainer
                    .InvokeWithinTransaction(true, ReadAndDeliverMessages(_outboxSettings, _endpointIdentity, _dependencyContainer), token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static Func<IAdvancedDatabaseTransaction, CancellationToken, Task> ReadAndDeliverMessages(
            OutboxSettings outboxSettings,
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer)
        {
            return async (transaction, token) =>
            {
                var outbox = dependencyContainer.Resolve<ITransactionalOutbox>();

                var messages = await ReadMessages(transaction, outboxSettings, endpointIdentity, token).ConfigureAwait(false);

                foreach (var message in messages)
                {
                    outbox.Add(message);
                }

                await outbox
                    .DeliverMessages(token)
                    .ConfigureAwait(false);
            };
        }

        private static async Task<IReadOnlyCollection<Messaging.IntegrationMessage>> ReadMessages(
            IDatabaseTransaction transaction,
            OutboxSettings settings,
            EndpointIdentity endpointIdentity,
            CancellationToken token)
        {
            var cutOff = DateTime.UtcNow - settings.OutboxDeliveryInterval;

            return (await transaction
                    .All<OutboxMessage>()
                    .Where(outbox => outbox.EndpointLogicalName == endpointIdentity.LogicalName
                                     && !outbox.Sent
                                     && outbox.Timestamp <= cutOff)
                    .Select(outbox => outbox.Message)
                    .CachedExpression("8270884D-CAB5-46DF-A541-7C0CEEFC9FA1")
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(BuildIntegrationMessage)
                .ToList();

            static Messaging.IntegrationMessage BuildIntegrationMessage(IntegrationMessage message)
            {
                var headers = message
                    .Headers
                    .Select(header => header.Payload)
                    .ToDictionary(header => header.GetType());

                return new Messaging.IntegrationMessage(message.Payload, TypeNode.FromString(message.ReflectedType), headers);
            }
        }
    }
}