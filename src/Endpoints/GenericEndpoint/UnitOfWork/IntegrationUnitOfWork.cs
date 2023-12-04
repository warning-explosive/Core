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
        private readonly ITransactionalOutbox _outbox;

        public IntegrationUnitOfWork(ITransactionalOutbox outbox)
        {
            _outbox = outbox;
        }

        protected override Task<EnUnitOfWorkBehavior> Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.FromResult(EnUnitOfWorkBehavior.Regular);
        }

        protected override async Task Commit(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await _outbox
                .DeliverMessages(token)
                .ConfigureAwait(false);
        }

        protected override Task Rollback(
            IAdvancedIntegrationContext context,
            Exception? exception,
            CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}