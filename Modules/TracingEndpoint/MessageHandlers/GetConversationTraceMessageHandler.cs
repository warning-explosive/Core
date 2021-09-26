namespace SpaceEngineers.Core.TracingEndpoint.MessageHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Reading;
    using DataAccess.Api.Transaction;
    using DatabaseModel;
    using Domain;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;

    [Component(EnLifestyle.Transient)]
    internal class GetConversationTraceMessageHandler : IMessageHandler<GetConversationTrace>,
                                                        ICollectionResolvable<IMessageHandler<GetConversationTrace>>
    {
        private readonly IIntegrationContext _integrationContext;
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;
        private readonly IStringFormatter _formatter;

        public GetConversationTraceMessageHandler(
            IIntegrationContext integrationContext,
            IDatabaseContext databaseContext,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            _integrationContext = integrationContext;
            _databaseContext = databaseContext;
            _serializer = serializer;
            _formatter = formatter;
        }

        public async Task Handle(GetConversationTrace query, CancellationToken token)
        {
            var capturedMessages = (await _databaseContext
                    .Read<CapturedMessageDatabaseEntity, Guid>()
                    .All()
                    .Where(captured => captured.Message.ConversationId == query.ConversationId)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(captured => new CapturedMessage(captured, _serializer, _formatter))
                .Select(captured => (captured, initiatorMessageId: captured.Message.ReadHeader<InitiatorMessageId>()?.Value))
                .ToList();

            if (capturedMessages.Any())
            {
                var entryPoint = capturedMessages
                    .Single(captured => captured.initiatorMessageId == null)
                    .captured;

                var groupByInitiatorId = capturedMessages
                    .Where(captured => captured.initiatorMessageId != null)
                    .GroupBy(info => info.initiatorMessageId, info => info.captured)
                    .ToDictionary(grp => grp.Key!.Value, grp => grp.ToArray());

                var reply = BuildTraceTree(query, entryPoint, groupByInitiatorId);

                await _integrationContext
                    .Reply(query, reply, token)
                    .ConfigureAwait(false);
            }
            else
            {
                var reply = new ConversationTrace(query.ConversationId);

                await _integrationContext
                    .Reply(query, reply, token)
                    .ConfigureAwait(false);
            }
        }

        private static ConversationTrace BuildTraceTree(
            GetConversationTrace query,
            CapturedMessage captured,
            IReadOnlyDictionary<Guid, CapturedMessage[]> groupByInitiatorId)
        {
            var subsequentTrace = (groupByInitiatorId.TryGetValue(captured.Message.Id, out var subsequentMessages)
                    ? subsequentMessages
                    : Array.Empty<CapturedMessage>())
                .Select(subsequent => BuildTraceTree(query, subsequent, groupByInitiatorId))
                .ToArray();

            return new ConversationTrace(query.ConversationId, captured.Message, captured.RefuseReason, subsequentTrace);
        }
    }
}