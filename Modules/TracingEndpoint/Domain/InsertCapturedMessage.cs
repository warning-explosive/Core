namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertCapturedMessage : IDatabaseStateTransformer<MessageCaptured>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public InsertCapturedMessage(IDatabaseContext databaseContext, IJsonSerializer serializer)
        {
            _serializer = serializer;
            _databaseContext = databaseContext;
        }

        public async Task Persist(MessageCaptured domainEvent, CancellationToken token)
        {
            var message = IntegrationMessage.Build(domainEvent.Message, _serializer);

            await _databaseContext
                .Write<IntegrationMessage, Guid>()
                .Insert(message, token)
                .ConfigureAwait(false);

            var capturedMessage = new DatabaseModel.CapturedMessage(Guid.NewGuid(), message, domainEvent.RefuseReason);

            await _databaseContext
                .Write<DatabaseModel.CapturedMessage, Guid>()
                .Insert(capturedMessage, token)
                .ConfigureAwait(false);
        }
    }
}