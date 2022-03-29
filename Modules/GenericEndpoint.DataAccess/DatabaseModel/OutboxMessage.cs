namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record OutboxMessage : BaseDatabaseEntity<Guid>
    {
        public OutboxMessage(
            Guid primaryKey,
            Guid outboxId,
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            bool sent)
            : base(primaryKey)
        {
            OutboxId = outboxId;
            EndpointIdentity = endpointIdentity;
            Message = message;
            Sent = sent;
        }

        public Guid OutboxId { get; private init; }

        public EndpointIdentity EndpointIdentity { get; private init; }

        public IntegrationMessage Message { get; private init; }

        public bool Sent { get; private init; }
    }
}