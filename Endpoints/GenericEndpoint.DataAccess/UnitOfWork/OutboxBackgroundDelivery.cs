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
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Extensions;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using Deduplication;
    using GenericEndpoint.UnitOfWork;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class OutboxBackgroundDelivery : IOutboxBackgroundDelivery,
                                              IResolvable<IOutboxBackgroundDelivery>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly Contract.EndpointIdentity _endpointIdentity;
        private readonly ISettingsProvider<OutboxSettings> _settingsProvider;
        private readonly IJsonSerializer _jsonSerializer;

        public OutboxBackgroundDelivery(
            IDependencyContainer dependencyContainer,
            Contract.EndpointIdentity endpointIdentity,
            ISettingsProvider<OutboxSettings> settingsProvider,
            IJsonSerializer jsonSerializer)
        {
            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _settingsProvider = settingsProvider;
            _jsonSerializer = jsonSerializer;
        }

        public async Task DeliverMessages(CancellationToken token)
        {
            var settings = await _settingsProvider
               .Get(token)
               .ConfigureAwait(false);

            var messages = await _dependencyContainer
               .InvokeWithinTransaction(true,
                    (_endpointIdentity, settings, _jsonSerializer),
                    ReadMessages,
                    token)
               .ConfigureAwait(false);

            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                await _dependencyContainer
                   .Resolve<IOutboxDelivery>()
                   .DeliverMessages(messages, token)
                   .ConfigureAwait(false);
            }

            static async Task<IReadOnlyCollection<Messaging.IntegrationMessage>> ReadMessages(
                IDatabaseTransaction transaction,
                (Contract.EndpointIdentity, OutboxSettings, IJsonSerializer) state,
                CancellationToken token)
            {
                var (endpointIdentity, settings, serializer) = state;
                var cutOff = DateTime.UtcNow - settings.OutboxDeliveryInterval;

                return (await transaction
                       .Read<OutboxMessage>()
                       .All()
                       .Where(outbox => outbox.EndpointIdentity.LogicalName == endpointIdentity.LogicalName
                                     && !outbox.Sent
                                     && outbox.Timestamp <= cutOff)
                       .Select(outbox => outbox.Message)
                       .ToListAsync(token)
                       .ConfigureAwait(false))
                   .Select(message => message.BuildIntegrationMessage(serializer))
                   .ToList();
            }
        }
    }
}