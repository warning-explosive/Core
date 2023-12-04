namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// InboxMessage
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    public record InboxMessage : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="message">Message</param>
        /// <param name="endpointLogicalName">Endpoint logical name</param>
        /// <param name="endpointInstanceName">Endpoint instance name</param>
        /// <param name="state">State</param>
        public InboxMessage(
            Guid primaryKey,
            IntegrationMessage message,
            string endpointLogicalName,
            string endpointInstanceName,
            EnInboxMessageState state)
            : base(primaryKey)
        {
            Message = message;
            EndpointLogicalName = endpointLogicalName;
            EndpointInstanceName = endpointInstanceName;
            State = state;
        }

        /// <summary>
        /// Message
        /// </summary>
        [ForeignKey(EnOnDeleteBehavior.Cascade)]
        public IntegrationMessage Message { get; set; }

        /// <summary>
        /// Endpoint logical name
        /// </summary>
        public string EndpointLogicalName { get; set; }

        /// <summary>
        /// Endpoint instance name
        /// </summary>
        public string EndpointInstanceName { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public EnInboxMessageState State { get; set; }
    }
}