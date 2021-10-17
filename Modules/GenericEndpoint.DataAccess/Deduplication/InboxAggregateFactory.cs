namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class InboxAggregateFactory : IAggregateFactory<Inbox, InboxAggregateSpecification>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;
        private readonly IStringFormatter _formatter;

        public InboxAggregateFactory(
            IDatabaseContext databaseContext,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
            _formatter = formatter;
        }

        public async Task<Inbox> Build(InboxAggregateSpecification spec, CancellationToken token)
        {
            var integrationMessageDatabaseEntity = await _databaseContext
                .Read<InboxMessage, Guid>()
                .All()
                .Where(message => message.Message.PrimaryKey == spec.Message.Id
                                  && message.EndpointIdentity.LogicalName == spec.EndpointIdentity.LogicalName
                                  && message.EndpointIdentity.InstanceName == spec.EndpointIdentity.InstanceName)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);

            return integrationMessageDatabaseEntity == null
                ? new Inbox(spec.Message, spec.EndpointIdentity)
                : new Inbox(integrationMessageDatabaseEntity, _serializer, _formatter);
        }
    }
}