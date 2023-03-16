namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Linq;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction;
    using SpaceEngineers.Core.GenericEndpoint.UnitOfWork;

    [Component(EnLifestyle.Singleton)]
    internal class OutboxBackgroundDelivery : IOutboxBackgroundDelivery,
                                              IResolvable<IOutboxBackgroundDelivery>
    {
        private readonly OutboxSettings _outboxSettings;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly Contract.EndpointIdentity _endpointIdentity;
        private readonly IOutboxDelivery _outboxDelivery;

        public OutboxBackgroundDelivery(
            ISettingsProvider<OutboxSettings> outboxSettingsProvider,
            IDependencyContainer dependencyContainer,
            Contract.EndpointIdentity endpointIdentity,
            IOutboxDelivery outboxDelivery)
        {
            _outboxSettings = outboxSettingsProvider.Get();

            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _outboxDelivery = outboxDelivery;
        }

        public async Task DeliverMessages(CancellationToken token)
        {
            var messages = await _dependencyContainer
               .InvokeWithinTransaction(true, (_endpointIdentity, _outboxSettings), ReadMessages, token)
               .ConfigureAwait(false);

            await _outboxDelivery
                .DeliverMessages(messages, token)
                .ConfigureAwait(false);

            static async Task<IReadOnlyCollection<Messaging.IntegrationMessage>> ReadMessages(
                IDatabaseTransaction transaction,
                (Contract.EndpointIdentity, OutboxSettings) state,
                CancellationToken token)
            {
                var (endpointIdentity, settings) = state;
                var cutOff = DateTime.UtcNow - settings.OutboxDeliveryInterval;

                return (await transaction
                       .All<Deduplication.OutboxMessage>()
                       .Where(outbox => outbox.EndpointLogicalName == endpointIdentity.LogicalName
                                     && !outbox.Sent
                                     && outbox.Timestamp <= cutOff)
                       .Select(outbox => outbox.Message)
                       .CachedExpression("8270884D-CAB5-46DF-A541-7C0CEEFC9FA1")
                       .ToListAsync(token)
                       .ConfigureAwait(false))
                   .Select(BuildIntegrationMessage)
                   .ToList();
            }

            static Messaging.IntegrationMessage BuildIntegrationMessage(Deduplication.IntegrationMessage message)
            {
                var headers = message
                    .Headers
                    .Select(header => header.Payload)
                    .ToDictionary(header => header.GetType());

                return new Messaging.IntegrationMessage(message.Payload, (TypeNode)message.ReflectedType, headers);
            }
        }
    }
}