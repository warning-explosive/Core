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
        /// <param name="reflectedType">Reflected type</param>
        /// <param name="headers">Headers</param>
        public IntegrationMessage(
            Guid primaryKey,
            IIntegrationMessage payload,
            SystemType reflectedType,
            IReadOnlyCollection<IntegrationMessageHeader> headers)
            : base(primaryKey)
        {
            Payload = payload;
            ReflectedType = reflectedType;
            Headers = headers;
        }

        internal IntegrationMessage(Messaging.IntegrationMessage message)
            : this(
                message.ReadRequiredHeader<Id>().Value,
                message.Payload,
                message.ReflectedType,
                message
                    .Headers
                    .Values
                    .Select(header => new IntegrationMessageHeader(Guid.NewGuid(), header))
                    .ToList())
        {
        }

        /// <summary>
        /// Payload
        /// </summary>
        [JsonColumn]
        public IIntegrationMessage Payload { get; set; }

        /// <summary>
        /// Reflected type
        /// </summary>
        public SystemType ReflectedType { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; set; }
    }
}