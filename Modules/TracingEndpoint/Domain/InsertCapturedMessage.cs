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

        public Task Persist(MessageCaptured domainEvent, CancellationToken token)
        {
            var message = IntegrationMessageDatabaseEntity.Build(domainEvent.Message, _serializer);

            return _databaseContext
                .Write<IntegrationMessageDatabaseEntity, Guid>()
                .Insert(message, token);
        }
    }
}