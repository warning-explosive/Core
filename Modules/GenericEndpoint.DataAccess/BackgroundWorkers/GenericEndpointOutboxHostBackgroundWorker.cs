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
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using Deduplication;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
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
            var unsent = await _dependencyContainer
                .InvokeWithinTransaction(GetUnsentMessages, token)
                .ConfigureAwait(false);

            await _dependencyContainer
                .InvokeWithinTransaction(unsent, DeliverMessages, token)
                .ConfigureAwait(false);
        }

        private async Task<IntegrationMessage[]> GetUnsentMessages(IDatabaseTransaction transaction, CancellationToken token)
        {
            var serializer = _dependencyContainer.Resolve<IJsonSerializer>();
            var formatter = _dependencyContainer.Resolve<IStringFormatter>();

            return (await transaction
                    .Read<OutboxMessage, Guid>()
                    .All()
                    .Where(outbox => !outbox.Sent)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(outbox => outbox.Message.BuildIntegrationMessage(serializer, formatter))
                .ToArray();
        }

        private async Task DeliverMessages(
            IDatabaseTransaction transaction,
            IntegrationMessage[] unsent,
            CancellationToken token)
        {
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();

            await Outbox
                .DeliverMessages(unsent, transport, transaction, token)
                .ConfigureAwait(false);
        }
    }
}