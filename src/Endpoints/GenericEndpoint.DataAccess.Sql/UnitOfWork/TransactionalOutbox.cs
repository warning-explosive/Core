namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.DataAccess.Orm.Sql.Linq;
    using Core.DataAccess.Orm.Sql.Transaction;
    using Deduplication;
    using GenericEndpoint.UnitOfWork;
    using Messaging.MessageHeaders;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

    [ComponentOverride]
    internal class TransactionalOutbox : ITransactionalOutbox,
                                         IResolvable<ITransactionalOutbox>,
                                         IDisposable
    {
        private readonly IIntegrationTransport _transport;
        private readonly IAdvancedDatabaseTransaction _transaction;

        private readonly List<Messaging.IntegrationMessage> _outgoingMessages;

        public TransactionalOutbox(
            IIntegrationTransport transport,
            IAdvancedDatabaseTransaction transaction)
        {
            _transport = transport;
            _transaction = transaction;

            _outgoingMessages = new List<Messaging.IntegrationMessage>();
        }

        public void Dispose()
        {
            _outgoingMessages.Clear();
        }

        public void Add(Messaging.IntegrationMessage message)
        {
            _outgoingMessages.Add(message);
        }

        public IReadOnlyCollection<Messaging.IntegrationMessage> All()
        {
            return _outgoingMessages;
        }

        public async Task DeliverMessages(CancellationToken token)
        {
            var sent = new List<Guid>(_outgoingMessages.Count);

            foreach (var message in _outgoingMessages)
            {
                var wasSent = await _transport
                    .Enqueue(message, token)
                    .ConfigureAwait(false);

                if (wasSent)
                {
                    sent.Add(message.ReadRequiredHeader<Id>().Value);
                }
            }

            if (!sent.Any())
            {
                return;
            }

            await _transaction
                .Update<OutboxMessage>()
                .Set(outbox => outbox.Sent.Assign(true))
                .Where(message => sent.Contains(message.PrimaryKey))
                .CachedExpression("6714446E-9263-42EA-A988-3B47941313BA")
                .Invoke(token)
                .ConfigureAwait(false);
        }
    }
}