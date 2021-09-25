namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contract.Abstractions;
    using Core.DataAccess.Api.Model;
    using CrossCuttingConcerns.Api.Abstractions;
    using Messaging;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record IntegrationMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageDatabaseEntity(
            Guid primaryKey,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> headers)
            : base(primaryKey)
        {
            Payload = payload;
            Headers = headers;
        }

        public JsonObject Payload { get; private init; }

        public IReadOnlyCollection<IntegrationMessageHeaderDatabaseEntity> Headers { get; private init; }

        public IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer, IStringFormatter formatter)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.Type);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToList();

            return new IntegrationMessage(PrimaryKey, payload, Payload.Type, headers, formatter);
        }

        public static IntegrationMessageDatabaseEntity Build(IntegrationMessage message, IJsonSerializer serializer)
        {
            var payload = new JsonObject(serializer.SerializeObject(message.Payload), message.Payload.GetType());

            var headers = message
                .Headers
                .Select(header => IntegrationMessageHeaderDatabaseEntity.Build(header, serializer))
                .ToList();

            return new IntegrationMessageDatabaseEntity(message.Id, payload, headers);
        }
    }
}