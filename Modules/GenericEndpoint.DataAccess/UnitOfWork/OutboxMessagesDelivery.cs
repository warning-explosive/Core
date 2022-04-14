namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Transaction;
    using Deduplication;
    using IntegrationTransport.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class OutboxMessagesDelivery : IOutboxMessagesDelivery,
                                            IResolvable<IOutboxMessagesDelivery>
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IIntegrationTransport _transport;

        public OutboxMessagesDelivery(
            IDatabaseTransaction transaction,
            IIntegrationTransport transport)
        {
            _transaction = transaction;
            _transport = transport;
        }

        public async Task DeliverMessages(Outbox outbox, CancellationToken token)
        {
            if (outbox.OutgoingMessages.Any())
            {
                await using (await _transaction.OpenScope(true, token).ConfigureAwait(false))
                {
                    await outbox
                       .DeliverMessages(_transport, token)
                       .ConfigureAwait(false);

                    await _transaction
                       .Track(outbox, token)
                       .ConfigureAwait(false);
                }
            }
        }
    }
}