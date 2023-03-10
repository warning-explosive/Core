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
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="message">Message</param>
        /// <param name="sent">Sent</param>
        public OutboxMessage(
            Guid primaryKey,
            Guid outboxId,
            DateTime timestamp,
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            bool sent)
            : base(primaryKey)
        {
            OutboxId = outboxId;
            Timestamp = timestamp;
            EndpointIdentity = endpointIdentity;
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
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; set; }

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