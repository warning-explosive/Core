namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Enumerations;
    using Basics.Primitives;
    using Pipeline;

    [Component(EnLifestyle.Scoped)]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork,
                                           IResolvable<IIntegrationUnitOfWork>
    {
        private readonly IOutboxDelivery _outboxDelivery;

        public IntegrationUnitOfWork(
            IOutboxStorage outboxStorage,
            IOutboxDelivery outboxDelivery)
        {
            _outboxDelivery = outboxDelivery;
            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        protected override Task<EnUnitOfWorkBehavior> Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.FromResult(EnUnitOfWorkBehavior.Regular);
        }

        protected override async Task Commit(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            try
            {
                await _outboxDelivery
                   .DeliverMessages(OutboxStorage.All(), token)
                   .ConfigureAwait(false);
            }
            finally
            {
                OutboxStorage.Clear();
            }
        }

        protected override Task Rollback(
            IAdvancedIntegrationContext context,
            Exception? exception,
            CancellationToken token)
        {
            OutboxStorage.Clear();
            return Task.CompletedTask;
        }
    }
}