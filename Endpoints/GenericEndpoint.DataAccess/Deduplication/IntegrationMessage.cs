namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contract.Abstractions;
    using Core.DataAccess.Api.Model;
    using Core.DataAccess.Api.Sql;
    using Core.DataAccess.Api.Sql.Attributes;
    using CrossCuttingConcerns.Json;
    using Messaging.MessageHeaders;

    /// <summary>
    /// IntegrationMessage
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    public record IntegrationMessage : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="payload">Payload</param>
        /// <param name="headers">Headers</param>
        public IntegrationMessage(
            Guid primaryKey,
            JsonObject payload,
            IReadOnlyCollection<IntegrationMessageHeader> headers)
            : base(primaryKey)
        {
            Payload = payload;
            Headers = headers;
        }

        /// <summary>
        /// Payload
        /// </summary>
        public JsonObject Payload { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; set; }

        internal Messaging.IntegrationMessage BuildIntegrationMessage(IJsonSerializer serializer)
        {
            var payload = (IIntegrationMessage)serializer.DeserializeObject(Payload.Value, Payload.SystemType);

            var headers = Headers
                .Select(header => header.BuildIntegrationMessageHeader(serializer))
                .ToDictionary(header => header.GetType());

            return new Messaging.IntegrationMessage(payload, Payload.SystemType, headers);
        }

        internal static IntegrationMessage Build(Messaging.IntegrationMessage message, IJsonSerializer serializer)
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