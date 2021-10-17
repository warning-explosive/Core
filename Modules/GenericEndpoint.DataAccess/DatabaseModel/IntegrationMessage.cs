namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contract.Abstractions;
    using Core.DataAccess.Api.Model;
    using CrossCuttingConcerns.Api.Abstractions;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record IntegrationMessage : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessage(
            Guid primaryKey,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeader> headers)
            : base(primaryKey)
        {
            Payload = payload;
            Headers = headers;
        }

        public JsonObject Payload { get; private init; }

        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; private init; }

        public Messaging.IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer, IStringFormatter formatter)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.SystemType);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToList();

            return new Messaging.IntegrationMessage(PrimaryKey, payload, Payload.SystemType, headers, formatter);
        }

        public static IntegrationMessage Build(Messaging.IntegrationMessage message, IJsonSerializer serializer)
        {
            var payload = new JsonObject(serializer.SerializeObject(message.Payload), message.Payload.GetType());

            var headers = message
                .Headers
                .Select(header => IntegrationMessageHeader.Build(header, serializer))
                .ToList();

            return new IntegrationMessage(message.Id, payload, headers);
        }
    }
}