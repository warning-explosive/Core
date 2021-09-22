namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.DatabaseEntity;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    internal class IntegrationMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageDatabaseEntity(
            Guid primaryKey,
            Guid messageId,
            Guid conversationId,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> headers)
            : base(primaryKey)
        {
            MessageId = messageId;
            ConversationId = conversationId;
            Payload = payload;
            Headers = headers;
        }

        public Guid MessageId { get; }

        public Guid ConversationId { get; }

        public JsonObject Payload { get; }

        public IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> Headers { get; }

        public IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer, IStringFormatter formatter)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.Type);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToList();

            return new IntegrationMessage(MessageId, payload, Payload.Type, headers, formatter);
        }

        public static IntegrationMessageDatabaseEntity Build(IntegrationMessage message, IJsonSerializer serializer)
        {
            var payload = new JsonObject(serializer.SerializeObject(message.Payload), message.Payload.GetType());

            var headers = message
                .Headers
                .Select(header => IntegrationMessageHeaderDatabaseEntity.Build(header, serializer))
                .ToList();

            var conversationId = message.ReadRequiredHeader<ConversationId>().Value;

            return new IntegrationMessageDatabaseEntity(Guid.NewGuid(), message.Id, conversationId, payload, headers);
        }
    }
}