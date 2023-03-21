namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot;
    using Deduplication;
    using Messaging.MessageHeaders;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Linq;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction;
    using SpaceEngineers.Core.GenericEndpoint.UnitOfWork;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [ComponentOverride]
    internal class OutboxDelivery : IOutboxDelivery,
                                    IResolvable<IOutboxDelivery>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IIntegrationTransport _transport;

        public OutboxDelivery(
            IDependencyContainer dependencyContainer,
            IIntegrationTransport transport)
        {
            _dependencyContainer = dependencyContainer;
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

            await _dependencyContainer
                .InvokeWithinTransaction(true, messages, DeliverMessages, token)
                .ConfigureAwait(false);
        }

        private async Task DeliverMessages(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
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

            if (!sent.Any())
            {
                return;
            }

            await transaction
                .Update<OutboxMessage>()
                .Set(outbox => outbox.Sent.Assign(true))
                .Where(message => sent.Contains(message.PrimaryKey))
                .CachedExpression("6714446E-9263-42EA-A988-3B47941313BA")
                .Invoke(token)
                .ConfigureAwait(false);
        }
    }
}