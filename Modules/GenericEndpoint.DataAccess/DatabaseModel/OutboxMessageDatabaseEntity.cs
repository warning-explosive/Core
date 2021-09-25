namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record OutboxMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public OutboxMessageDatabaseEntity(
            Guid primaryKey,
            IntegrationMessageDatabaseEntity message,
            bool sent)
            : base(primaryKey)
        {
            Message = message;
            Sent = sent;
        }

        public IntegrationMessageDatabaseEntity Message { get; private init; }

        public bool Sent { get; private init; }
    }
}