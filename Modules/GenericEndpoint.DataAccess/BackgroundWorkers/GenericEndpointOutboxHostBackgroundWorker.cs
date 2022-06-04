namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.BackgroundWorkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Extensions;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using DatabaseModel;
    using GenericEndpoint.UnitOfWork;
    using GenericHost.Api.Abstractions;
    using Settings;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    internal class GenericEndpointOutboxHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointOutboxHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var settings = await _dependencyContainer
               .Resolve<ISettingsProvider<OutboxSettings>>()
               .Get(token)
               .ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(settings.OutboxDeliveryInterval, token).ConfigureAwait(false);

                await DeliverMessages(token).ConfigureAwait(false);
            }
        }

        private async Task DeliverMessages(CancellationToken token)
        {
            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();
            var jsonSerializer = _dependencyContainer.Resolve<IJsonSerializer>();

            var messages = await _dependencyContainer
                .InvokeWithinTransaction(
                    true,
                    (endpointIdentity, jsonSerializer),
                    Read,
                    token)
                .ConfigureAwait(false);

            var outboxDelivery = _dependencyContainer.Resolve<IOutboxDelivery>();

            await outboxDelivery
               .DeliverMessages(messages, token)
               .ConfigureAwait(false);
        }

        private static async Task<IReadOnlyCollection<IntegrationMessage>> Read(
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
}