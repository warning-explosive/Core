namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Linq;
    using Core.DataAccess.Orm.Transaction;
    using CrossCuttingConcerns.Settings;
    using Deduplication;
    using GenericEndpoint.UnitOfWork;
    using Settings;

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
                       .All<OutboxMessage>()
                       .Where(outbox => outbox.EndpointIdentity.LogicalName == endpointIdentity.LogicalName
                                     && !outbox.Sent
                                     && outbox.Timestamp <= cutOff)
                       .Select(outbox => outbox.Message)
                       .CachedExpression("8270884D-CAB5-46DF-A541-7C0CEEFC9FA1")
                       .ToListAsync(token)
                       .ConfigureAwait(false))
                   .Select(BuildIntegrationMessage)
                   .ToList();
            }

            static Messaging.IntegrationMessage BuildIntegrationMessage(IntegrationMessage message)
            {
                var headers = message
                    .Headers
                    .Select(header => header.Payload)
                    .ToDictionary(header => header.GetType());

                return new Messaging.IntegrationMessage(message.Payload, message.ReflectedType, headers);
            }
        }
    }
}