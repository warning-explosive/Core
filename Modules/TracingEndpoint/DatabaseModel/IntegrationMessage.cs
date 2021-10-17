namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Model;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record IntegrationMessage : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessage(
            Guid primaryKey,
            Guid messageId,
            Guid conversationId,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeader> headers)
            : base(primaryKey)
        {
            MessageId = messageId;
            ConversationId = conversationId;
            Payload = payload;
            Headers = headers;
        }

        public Guid MessageId { get; private init; }

        public Guid ConversationId { get; private init; }

        public JsonObject Payload { get; private init; }

        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; private init; }

        public GenericEndpoint.Messaging.IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer, IStringFormatter formatter)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.SystemType);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToList();

            return new GenericEndpoint.Messaging.IntegrationMessage(MessageId, payload, Payload.SystemType, headers, formatter);
        }

        public static IntegrationMessage Build(GenericEndpoint.Messaging.IntegrationMessage message, IJsonSerializer serializer)
        {
            var payload = new JsonObject(serializer.SerializeObject(message.Payload), message.Payload.GetType());

            var headers = message
                .Headers
                .Select(header => IntegrationMessageHeader.Build(header, serializer))
                .ToList();

            var conversationId = message.ReadRequiredHeader<ConversationId>().Value;

            return new IntegrationMessage(Guid.NewGuid(), message.Id, conversationId, payload, headers);
        }
    }
}