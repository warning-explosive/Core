namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Core.DataAccess.Api.Transaction;
    using DatabaseModel;
    using GenericEndpoint.UnitOfWork;
    using IntegrationTransport.Api.Abstractions;
    using Messaging.MessageHeaders;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [ComponentOverride]
    internal class OutboxDelivery : IOutboxDelivery,
                                    IResolvable<IOutboxDelivery>
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IIntegrationTransport _transport;

        public OutboxDelivery(
            IDatabaseTransaction transaction,
            IIntegrationTransport transport)
        {
            _transaction = transaction;
            _transport = transport;
        }

        public async Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            if (!messages.Any())
            {
                return;
            }

            await using (await _transaction.OpenScope(true, token).ConfigureAwait(false))
            {
                var sent = new List<Guid>(messages.Count);

                foreach (var message in messages)
                {
                    var wasSent = await _transport
                       .Enqueue(message, token)
                       .ConfigureAwait(false);

                    if (wasSent)
                    {
                        sent.Add(message.ReadRequiredHeader<Id>().Value);
                    }
                }

                await _transaction
                   .Write<OutboxMessage, Guid>()
                   .Update(sent, outbox => outbox.Sent, true, token)
                   .ConfigureAwait(false);
            }
        }
    }
}