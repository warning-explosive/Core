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
                .Select(captured => new Domain.CapturedMessage(captured))
                .ToList();

            if (capturedMessages.Any())
            {
                var entryPoint = capturedMessages
                    .Single(captured => captured.SerializedMessage.InitiatorMessageId == null);

                var groupByInitiatorId = capturedMessages
                    .Where(captured => captured.SerializedMessage.InitiatorMessageId != null)
                    .GroupBy(captured => captured.SerializedMessage.InitiatorMessageId)
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
            Domain.CapturedMessage captured,
            IReadOnlyDictionary<Guid, Domain.CapturedMessage[]> groupByInitiatorId)
        {
            var subsequentTrace = (groupByInitiatorId.TryGetValue(captured.SerializedMessage.Id, out var subsequentMessages)
                    ? subsequentMessages
                    : Array.Empty<Domain.CapturedMessage>())
                .Select(subsequent => BuildTraceTree(query, subsequent, groupByInitiatorId))
                .ToArray();

            return new ConversationTrace(query.ConversationId, captured.SerializedMessage, captured.RefuseReason, subsequentTrace);
        }
    }
}