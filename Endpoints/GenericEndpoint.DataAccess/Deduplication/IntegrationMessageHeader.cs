namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Sql;
    using Core.DataAccess.Api.Sql.Attributes;
    using Messaging.MessageHeaders;

    /// <summary>
    /// IntegrationMessageHeader
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    public record IntegrationMessageHeader : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="payload">Payload</param>
        public IntegrationMessageHeader(Guid primaryKey, IIntegrationMessageHeader payload)
            : base(primaryKey)
        {
            Payload = payload;
        }

        /// <summary>
        /// Payload
        /// </summary>
        [JsonColumn]
        public IIntegrationMessageHeader Payload { get; set; }
    }
}