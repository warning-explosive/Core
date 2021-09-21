namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Exceptions;
    using DataAccess.Api.Reading;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class CapturedMessageAggregateFactory : IAggregateFactory<CapturedMessage, CapturedMessageSpecification>
    {
        private readonly IReadRepository<IntegrationMessageDatabaseEntity, Guid> _integrationMessageReadRepository;
        private readonly IReadRepository<RefusedIntegrationMessages, Guid> _refusedIntegrationMessageReadRepository;
        private readonly IJsonSerializer _serializer;
        private readonly IStringFormatter _formatter;

        public CapturedMessageAggregateFactory(
            IReadRepository<IntegrationMessageDatabaseEntity, Guid> integrationMessageReadRepository,
            IReadRepository<RefusedIntegrationMessages, Guid> refusedIntegrationMessageReadRepository,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            _integrationMessageReadRepository = integrationMessageReadRepository;
            _serializer = serializer;
            _formatter = formatter;
            _refusedIntegrationMessageReadRepository = refusedIntegrationMessageReadRepository;
        }

        public async Task<CapturedMessage> Build(CapturedMessageSpecification spec, CancellationToken token)
        {
            var integrationMessageDatabaseEntity = await _integrationMessageReadRepository
                .SingleOrDefaultAsync(spec.IntegrationMessageId, token)
                .ConfigureAwait(false);

            var refuseReason = await _refusedIntegrationMessageReadRepository
                .All()
                .Where(refused => refused.Message.PrimaryKey == spec.IntegrationMessageId)
                .Select(refused => refused.RefuseReason)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);

            return integrationMessageDatabaseEntity != null
                ? new CapturedMessage(integrationMessageDatabaseEntity, refuseReason, _serializer, _formatter)
                : throw new EntityNotFoundException<IntegrationMessageDatabaseEntity, Guid>(spec.IntegrationMessageId);
        }
    }
}