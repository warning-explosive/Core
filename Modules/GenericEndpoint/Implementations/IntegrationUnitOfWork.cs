namespace SpaceEngineers.Core.GenericEndpoint.Implementations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using IntegrationTransport.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork
    {
        private readonly IIntegrationTransport _transport;

        public IntegrationUnitOfWork(IIntegrationTransport transport, IOutboxStorage outboxStorage)
        {
            _transport = transport;
            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        protected override Task Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return DeliverAll(token);
        }

        protected override Task Rollback(IAdvancedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task DeliverAll(CancellationToken token)
        {
            foreach (var message in OutboxStorage.All)
            {
                await _transport.Enqueue(message, token).ConfigureAwait(false);
                await OutboxStorage.Ack(message, token).ConfigureAwait(false);
            }
        }
    }
}