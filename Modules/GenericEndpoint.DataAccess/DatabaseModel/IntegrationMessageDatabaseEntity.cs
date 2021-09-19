namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contract.Abstractions;
    using Core.DataAccess.Api;
    using Core.DataAccess.Api.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using Messaging;
    using Messaging.Abstractions;

    internal class IntegrationMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageDatabaseEntity(
            Guid primaryKey,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> headers,
            IntegrationMessageDatabaseEntity? initiator,
            bool isError,
            bool sent,
            bool handled,
            string handledByEndpoint)
            : base(primaryKey)
        {
            Payload = payload;
            Headers = headers;
            Initiator = initiator;
            IsError = isError;
            Sent = sent;
            Handled = handled;
            HandledByEndpoint = handledByEndpoint;
        }

        public JsonObject Payload { get; }

        public IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> Headers { get; }

        public IntegrationMessageDatabaseEntity? Initiator { get; }

        public bool IsError { get; }

        public bool Sent { get; }

        public bool Handled { get; }

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