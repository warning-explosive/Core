namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Abstractions;
    using Core.DataAccess.Api.Extensions;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class InboxAggregateFactory : IAggregateFactory<Inbox, InboxAggregateSpecification>
    {
        private readonly IReadRepository<IntegrationMessageDatabaseEntity, Guid> _integrationMessageReadRepository;
        private readonly IJsonSerializer _serializer;
        private readonly IStringFormatter _formatter;

        public InboxAggregateFactory(
            IReadRepository<IntegrationMessageDatabaseEntity, Guid> integrationMessageReadRepository,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            _integrationMessageReadRepository = integrationMessageReadRepository;
            _serializer = serializer;
            _formatter = formatter;
        }

        public async Task<Inbox> Build(InboxAggregateSpecification spec, CancellationToken token)
        {
            var integrationMessageDatabaseEntity = await _integrationMessageReadRepository
                .All()
                .Where(message => message.PrimaryKey == spec.Message.Id && message.HandledByEndpoint == spec.EndpointIdentity.LogicalName)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);

            Inbox inbox;

            if (integrationMessageDatabaseEntity == null)
            {
                inbox = new Inbox(spec.Message);
            }
            else
            {
                var message = integrationMessageDatabaseEntity.BuildIntegrationMessage(_serializer, _formatter);

                inbox = new Inbox(message);

                if (integrationMessageDatabaseEntity.Handled)
                {
                    inbox.MarkAsHandled();
                }

                if (integrationMessageDatabaseEntity.IsError)
                {
                    inbox.MarkAsError();
                }
            }

            return inbox;
        }
    }
}