namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using Deduplication;
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
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var serializer = _dependencyContainer.Resolve<IJsonSerializer>();
            var formatter = _dependencyContainer.Resolve<IStringFormatter>();

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var transaction = _dependencyContainer.Resolve<IDatabaseTransaction>();

                var outbox = await AllUnsent(transaction, serializer, formatter, token)
                    .ConfigureAwait(false);

                await outbox
                    .DeliverMessages(transport, transaction, token)
                    .ConfigureAwait(false);
            }
        }

        private async Task<Outbox> AllUnsent(
            IDatabaseTransaction transaction,
            IJsonSerializer serializer,
            IStringFormatter formatter,
            CancellationToken token)
        {
            await using (await transaction.Open(true, token).ConfigureAwait(false))
            {
                var subsequentMessages = (await _dependencyContainer
                        .Resolve<IReadRepository<OutboxMessageDatabaseEntity, Guid>>()
                        .All()
                        .Where(outbox => !outbox.Sent)
                        .ToListAsync(token)
                        .ConfigureAwait(false))
                    .Select(outbox => outbox.Message.BuildIntegrationMessage(serializer, formatter))
                    .ToList();

                return new Outbox(subsequentMessages);
            }
        }
    }
}