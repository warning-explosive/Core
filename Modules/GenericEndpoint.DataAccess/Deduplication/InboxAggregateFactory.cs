namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Json;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;
    using Messaging.MessageHeaders;

    [Component(EnLifestyle.Scoped)]
    internal class InboxAggregateFactory : IAggregateFactory<Inbox, InboxAggregateSpecification>,
                                           IResolvable<IAggregateFactory<Inbox, InboxAggregateSpecification>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public InboxAggregateFactory(
            IDatabaseContext databaseContext,
            IJsonSerializer serializer)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
        }

        public async Task<Inbox> Build(InboxAggregateSpecification spec, CancellationToken token)
        {
            var inboxMessageDatabaseEntity = await _databaseContext
                .Read<InboxMessage, Guid>()
                .All()
                .Where(message => message.Message.PrimaryKey == spec.Message.ReadRequiredHeader<Id>().Value
                                  && message.EndpointIdentity.LogicalName == spec.EndpointIdentity.LogicalName
                                  && message.EndpointIdentity.InstanceName == spec.EndpointIdentity.InstanceName)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);

            return inboxMessageDatabaseEntity == null
                ? new Inbox(spec.Message, spec.EndpointIdentity)
                : new Inbox(inboxMessageDatabaseEntity, _serializer);
        }
    }
}