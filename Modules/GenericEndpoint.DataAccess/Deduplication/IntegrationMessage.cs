namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contract.Abstractions;
    using Core.DataAccess.Api.Model;
    using CrossCuttingConcerns.Json;
    using Messaging.MessageHeaders;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
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

        public JsonObject Payload { get; set; }

        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; set; }

        public Messaging.IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.SystemType);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToDictionary(header => header.GetType());

            return new Messaging.IntegrationMessage(payload, Payload.SystemType, headers);
        }

        public static IntegrationMessage Build(Messaging.IntegrationMessage message, IJsonSerializer serializer)
        {
            var payload = new JsonObject(serializer.SerializeObject(message.Payload), message.Payload.GetType());

            var headers = message
               .Headers
               .Values
               .Select(header => IntegrationMessageHeader.Build(header, serializer))
               .ToList();

            return new IntegrationMessage(message.ReadRequiredHeader<Id>().Value, payload, headers);
        }
    }
}