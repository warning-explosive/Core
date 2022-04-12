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
    using EndpointIdentity = Contract.EndpointIdentity;

    [Component(EnLifestyle.Scoped)]
    internal class OutboxAggregateFactory : IAggregateFactory<Outbox, OutboxAggregateSpecification>,
                                            IResolvable<IAggregateFactory<Outbox, OutboxAggregateSpecification>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public OutboxAggregateFactory(
            EndpointIdentity endpointIdentity,
            IDatabaseContext databaseContext,
            IJsonSerializer serializer)
        {
            _endpointIdentity = endpointIdentity;
            _databaseContext = databaseContext;
            _serializer = serializer;
        }

        public async Task<Outbox> Build(OutboxAggregateSpecification spec, CancellationToken token)
        {
            var unsentMessages = await _databaseContext
                .Read<OutboxMessage, Guid>()
                .All()
                .Where(outbox => outbox.OutboxId == spec.OutboxId
                              && outbox.EndpointIdentity.LogicalName == _endpointIdentity.LogicalName
                              && !outbox.Sent)
                .Select(outbox => outbox.Message)
                .ToListAsync(token)
                .ConfigureAwait(false);

            return new Outbox(spec.OutboxId, unsentMessages, _serializer);
        }
    }
}