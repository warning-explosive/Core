namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertCapturedMessage : IDomainEventHandler<MessageCaptured>,
                                           IResolvable<IDomainEventHandler<MessageCaptured>>
    {
        private readonly IDatabaseContext _databaseContext;

        public InsertCapturedMessage(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task Handle(MessageCaptured domainEvent, CancellationToken token)
        {
            var message = IntegrationMessage.Build(domainEvent.SerializedMessage);

            var capturedMessage = new DatabaseModel.CapturedMessage(Guid.NewGuid(), message, domainEvent.RefuseReason);

            await _databaseContext
                .Write<DatabaseModel.CapturedMessage, Guid>()
                .Insert(capturedMessage, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);
        }
    }
}