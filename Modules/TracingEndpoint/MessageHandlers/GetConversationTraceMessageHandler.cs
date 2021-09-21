namespace SpaceEngineers.Core.TracingEndpoint.MessageHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract.Messages;
    using DataAccess.Api.Reading;
    using DatabaseModel;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;

    [Component(EnLifestyle.Transient)]
    internal class GetConversationTraceMessageHandler : IMessageHandler<GetConversationTrace>,
                                                        ICollectionResolvable<IMessageHandler<GetConversationTrace>>
    {
        private readonly IIntegrationContext _integrationContext;
        private readonly IReadRepository<IntegrationMessageDatabaseEntity, Guid> _integrationMessageReadRepository;
        private readonly IAggregateFactory<CapturedMessage, CapturedMessageSpecification> _capturedMessageFactory;

        public GetConversationTraceMessageHandler(
            IIntegrationContext integrationContext,
            IReadRepository<IntegrationMessageDatabaseEntity, Guid> integrationMessageReadRepository,
            IAggregateFactory<CapturedMessage, CapturedMessageSpecification> capturedMessageFactory)
        {
            _integrationContext = integrationContext;
            _integrationMessageReadRepository = integrationMessageReadRepository;
            _capturedMessageFactory = capturedMessageFactory;
        }

        public async Task Handle(GetConversationTrace query, CancellationToken token)
        {
            var primaryKeys = await _integrationMessageReadRepository
                .All()
                .Where(message => message.ConversationId == query.ConversationId)
                .Select(message => message.PrimaryKey)
                .ToListAsync(token)
                .ConfigureAwait(false);

            var capturedMessages = BuildCapturedMessages(primaryKeys, token)
                .AsEnumerable(token)
                .ToList();

            if (capturedMessages.Any())
            {
                var groupByInitiatorId = capturedMessages
                    .Select(captured =>
                    {
                        var initiatorMessageId = captured.Message.ReadHeader<InitiatorMessageId>()?.Value;
                        var message = captured;

                        return (initiatorMessageId, message);
                    })
                    .Where(info => info.initiatorMessageId != null)
                    .GroupBy(info => info.initiatorMessageId, info => info.message)
                    .ToDictionary(grp => grp.Key!.Value, grp => grp.ToArray());

                var entryPoint =
                    capturedMessages.Single(captured => captured.Message.ReadHeader<InitiatorMessageId>() == null);

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

        private async IAsyncEnumerable<CapturedMessage> BuildCapturedMessages(List<Guid> primaryKeys, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var primaryKey in primaryKeys)
            {
                yield return await _capturedMessageFactory
                    .Build(new CapturedMessageSpecification(primaryKey), token)
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