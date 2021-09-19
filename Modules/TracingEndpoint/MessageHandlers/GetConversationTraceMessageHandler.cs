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
    using Domain;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;
    using Repositories;

    [Component(EnLifestyle.Transient)]
    internal class GetConversationTraceMessageHandler : IMessageHandler<GetConversationTrace>,
                                                        ICollectionResolvable<IMessageHandler<GetConversationTrace>>
    {
        private readonly IIntegrationContext _integrationContext;
        private readonly ICapturedMessageReadRepository _repository;

        public GetConversationTraceMessageHandler(
            IIntegrationContext integrationContext,
            ICapturedMessageReadRepository repository)
        {
            _integrationContext = integrationContext;
            _repository = repository;
        }

        public Task Handle(GetConversationTrace query, CancellationToken token)
        {
            var capturedMessages = _repository
                .Read(query.ConversationId)
                .ToList();

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

            var entryPoint = capturedMessages.Single(captured => captured.Message.ReadHeader<InitiatorMessageId>() == null);

            var reply = BuildTraceTree(query, entryPoint, groupByInitiatorId);

            return _integrationContext.Reply(query, reply, token);
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