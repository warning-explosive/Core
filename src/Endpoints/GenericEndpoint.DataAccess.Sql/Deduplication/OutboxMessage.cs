namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// OutboxMessage
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    public record OutboxMessage : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="outboxId">OutboxId</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="endpointLogicalName">Endpoint logical name</param>
        /// <param name="endpointInstanceName">Endpoint instance name</param>
        /// <param name="message">Message</param>
        /// <param name="sent">Sent</param>
        public OutboxMessage(
            Guid primaryKey,
            Guid outboxId,
            DateTime timestamp,
            string endpointLogicalName,
            string endpointInstanceName,
            IntegrationMessage message,
            bool sent)
            : base(primaryKey)
        {
            OutboxId = outboxId;
            Timestamp = timestamp;
            EndpointLogicalName = endpointLogicalName;
            EndpointInstanceName = endpointInstanceName;
            Message = message;
            Sent = sent;
        }

        /// <summary>
        /// OutboxId
        /// </summary>
        public Guid OutboxId { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Endpoint logical name
        /// </summary>
        public string EndpointLogicalName { get; set; }

        /// <summary>
        /// Endpoint instance name
        /// </summary>
        public string EndpointInstanceName { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        [ForeignKey(EnOnDeleteBehavior.Cascade)]
        public IntegrationMessage Message { get; set; }

        /// <summary>
        /// Sent
        /// </summary>
        public bool Sent { get; set; }
    }
}