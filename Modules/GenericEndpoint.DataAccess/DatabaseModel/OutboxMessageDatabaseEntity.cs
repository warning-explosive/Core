namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using Core.DataAccess.Api.DatabaseEntity;

    internal class OutboxMessageDatabaseEntity : BaseDatabaseEntity<Guid>
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

        public IntegrationMessageDatabaseEntity Message { get; }

        public bool Sent { get; }
    }
}