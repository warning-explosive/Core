namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Sql;
    using Core.DataAccess.Api.Sql.Attributes;

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
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="isError">IsError</param>
        /// <param name="handled">Handled</param>
        public InboxMessage(
            Guid primaryKey,
            IntegrationMessage message,
            EndpointIdentity endpointIdentity,
            bool isError,
            bool handled)
            : base(primaryKey)
        {
            Message = message;
            EndpointIdentity = endpointIdentity;
            IsError = isError;
            Handled = handled;
        }

        /// <summary>
        /// Message
        /// </summary>
        public IntegrationMessage Message { get; set; }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; set; }

        /// <summary>
        /// IsError
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Handled
        /// </summary>
        public bool Handled { get; set; }
    }
}