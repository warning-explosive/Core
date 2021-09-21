namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.DatabaseEntity;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;

    internal class IntegrationMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageDatabaseEntity(
            Guid conversationId,
            Guid primaryKey,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> headers,
            IntegrationMessageDatabaseEntity? initiator,
            string handledByEndpoint)
            : base(primaryKey)
        {
            Payload = payload;
            Headers = headers;
            Initiator = initiator;
            HandledByEndpoint = handledByEndpoint;
            ConversationId = conversationId;
        }

        public Guid ConversationId { get; }

        public JsonObject Payload { get; }

        public IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> Headers { get; }

        public IntegrationMessageDatabaseEntity? Initiator { get; }

        public string? HandledByEndpoint { get; }

        public IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer, IStringFormatter formatter)
        {
            var payload = Deserialize<IIntegrationMessage>(Payload, serializer);

            var headers = Headers
                .Select(header => Deserialize<IIntegrationMessageHeader>(header.Value, serializer))
                .ToList();

            return new IntegrationMessage(PrimaryKey, payload, Payload.Type, headers, formatter);

            static T Deserialize<T>(JsonObject jsonObject, IJsonSerializer serializer)
            {
                return (T)serializer.DeserializeObject(jsonObject.Value, jsonObject.Type);
            }
        }
    }
}