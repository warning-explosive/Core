namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Deduplication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Contract.Abstractions;
    using Messaging.MessageHeaders;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

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
            Type reflectedType,
            IReadOnlyCollection<IntegrationMessageHeader> headers)
            : base(primaryKey)
        {
            Payload = payload;
            ReflectedType = (TypeNode)reflectedType;
            Headers = headers;
        }

        internal IntegrationMessage(Messaging.IntegrationMessage message)
            : this(
                message.ReadRequiredHeader<Id>().Value,
                message.Payload,
                message.ReflectedType,
                Array.Empty<IntegrationMessageHeader>())
        {
            Headers = message
                .Headers
                .Values
                .Select(header => new IntegrationMessageHeader(Guid.NewGuid(), this, header))
                .ToList();
        }

        /// <summary>
        /// Payload
        /// </summary>
        [JsonColumn]
        public IIntegrationMessage Payload { get; set; }

        /// <summary>
        /// Reflected type
        /// </summary>
        public string ReflectedType { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public IReadOnlyCollection<IntegrationMessageHeader> Headers { get; set; }
    }
}