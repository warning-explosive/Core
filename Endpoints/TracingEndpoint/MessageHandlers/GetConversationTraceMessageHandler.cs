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
    using DataAccess.Api.Reading;
    using DataAccess.Api.Transaction;
    using GenericEndpoint.Api.Abstractions;
    using CapturedMessage = DatabaseModel.CapturedMessage;

    [Component(EnLifestyle.Transient)]
    internal class GetConversationTraceMessageHandler : IMessageHandler<GetConversationTrace>,
                                                        IResolvable<IMessageHandler<GetConversationTrace>>
    {
        private readonly IIntegrationContext _integrationContext;
        private readonly IDatabaseContext _databaseContext;

        public GetConversationTraceMessageHandler(
            IIntegrationContext integrationContext,
            IDatabaseContext databaseContext)
        {
            _integrationContext = integrationContext;
            _databaseContext = databaseContext;
        }

        public async Task Handle(GetConversationTrace query, CancellationToken token)
        {
            var capturedMessages = (await _databaseContext
                    .Read<CapturedMessage, Guid>()
                    .All()
                    .Where(captured => captured.Message.ConversationId == query.ConversationId)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .ToList();

            ConversationTrace reply;

            if (capturedMessages.Any())
            {
                var entryPoint = capturedMessages
                    .Single(captured => captured.Message.InitiatorMessageId == null);

                var groupByInitiatorId = capturedMessages
                    .Where(captured => captured.Message.InitiatorMessageId != null)
                    .GroupBy(captured => captured.Message.InitiatorMessageId)
                    .ToDictionary(
                        grp => grp.Key!.Value,
                        grp => grp.ToArray());

                reply = BuildTraceTree(query, entryPoint, groupByInitiatorId);
            }
            else
            {
                reply = new ConversationTrace(query.ConversationId);
            }

            await _integrationContext
               .Reply(query, reply, token)
               .ConfigureAwait(false);
        }

        private static ConversationTrace BuildTraceTree(
            GetConversationTrace query,
            CapturedMessage captured,
            IReadOnlyDictionary<Guid, CapturedMessage[]> groupByInitiatorId)
        {
            var subsequentTrace = (groupByInitiatorId.TryGetValue(captured.Message.MessageId, out var subsequentMessages)
                    ? subsequentMessages
                    : Array.Empty<CapturedMessage>())
                .Select(subsequent => BuildTraceTree(query, subsequent, groupByInitiatorId))
                .ToArray();

            return new ConversationTrace(
                query.ConversationId,
                captured.Message.BuildSerializedIntegrationMessage(),
                captured.RefuseReason,
                subsequentTrace);
        }
    }
}