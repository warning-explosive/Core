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
    internal class OutboxAggregateFactory : IAggregateFactory<Outbox, OutboxAggregateSpecification>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;
        private readonly IStringFormatter _formatter;

        public OutboxAggregateFactory(
            IDatabaseContext databaseContext,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
            _formatter = formatter;
        }

        public async Task<Outbox> Build(OutboxAggregateSpecification spec, CancellationToken token)
        {
            var unsentMessages = await _databaseContext
                .Read<OutboxMessage, Guid>()
                .All()
                .Where(outbox => outbox.OutboxId == spec.OutboxId && !outbox.Sent)
                .Select(outbox => outbox.Message)
                .ToListAsync(token)
                .ConfigureAwait(false);

            return new Outbox(spec.OutboxId, unsentMessages, _serializer, _formatter);
        }
    }
}