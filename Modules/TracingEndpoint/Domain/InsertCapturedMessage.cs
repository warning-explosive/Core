namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Persisting;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertCapturedMessage : IDatabaseStateTransformer<MessageCaptured>
    {
        private readonly IRepository<IntegrationMessageDatabaseEntity, Guid> _integrationMessageReadRepository;
        private readonly IJsonSerializer _serializer;

        public InsertCapturedMessage(
            IRepository<IntegrationMessageDatabaseEntity, Guid> integrationMessageReadRepository,
            IJsonSerializer serializer)
        {
            _serializer = serializer;
            _integrationMessageReadRepository = integrationMessageReadRepository;
        }

        public Task Persist(MessageCaptured domainEvent, CancellationToken token)
        {
            var message = IntegrationMessageDatabaseEntity.Build(domainEvent.Message, _serializer);
            return _integrationMessageReadRepository.Insert(message, token);
        }
    }
}