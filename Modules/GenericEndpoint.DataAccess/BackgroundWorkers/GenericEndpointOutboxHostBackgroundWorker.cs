namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.BackgroundWorkers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Extensions;
    using DatabaseModel;
    using Deduplication;
    using GenericDomain.Api.Abstractions;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;

    internal class GenericEndpointOutboxHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointOutboxHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // TODO: #154 - add polling timeout
                await Task.Delay(TimeSpan.FromSeconds(42), token).ConfigureAwait(false);
                await DeliverMessages(token).ConfigureAwait(false);
            }
        }

        private Task DeliverMessages(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DeliverMessagesUnsafe)
                .Catch<Exception>()
                .Invoke(token);
        }

        private async Task DeliverMessagesUnsafe(CancellationToken token)
        {
            await _dependencyContainer
                .InvokeWithinTransaction(true, DeliverMessages, token)
                .ConfigureAwait(false);
        }

        private async Task DeliverMessages(
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            var unsent = await transaction
                .Read<OutboxMessage, Guid>()
                .All()
                .Where(outbox => !outbox.Sent)
                .Select(outbox => outbox.OutboxId)
                .Distinct()
                .ToListAsync(token)
                .ConfigureAwait(false);

            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();

            foreach (var outboxId in unsent)
            {
                var outbox = await _dependencyContainer
                    .Resolve<IAggregateFactory<Outbox, OutboxAggregateSpecification>>()
                    .Build(new OutboxAggregateSpecification(outboxId), token)
                    .ConfigureAwait(false);

                await outbox
                    .DeliverMessages(transport, token)
                    .ConfigureAwait(false);

                await transaction
                    .Track(outbox, token)
                    .ConfigureAwait(false);
            }
        }
    }
}