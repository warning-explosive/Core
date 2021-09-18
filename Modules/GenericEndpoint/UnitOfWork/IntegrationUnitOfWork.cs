namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Enumerations;
    using Basics.Primitives;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Pipeline;

    [Component(EnLifestyle.Scoped)]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork
    {
        private readonly IIntegrationTransport _transport;

        public IntegrationUnitOfWork(
            IIntegrationTransport transport,
            IOutboxStorage outboxStorage)
        {
            _transport = transport;
            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        protected override Task<EnUnitOfWorkBehavior> Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.FromResult(EnUnitOfWorkBehavior.Regular);
        }

        protected override async Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await DeliverAll(OutboxStorage.All(), token).ConfigureAwait(false);
        }

        protected override Task Rollback(IAdvancedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task DeliverAll(IReadOnlyCollection<IntegrationMessage> outgoingMessages, CancellationToken token)
        {
            foreach (var message in outgoingMessages)
            {
                await _transport.Enqueue(message, token).ConfigureAwait(false);
            }
        }
    }
}