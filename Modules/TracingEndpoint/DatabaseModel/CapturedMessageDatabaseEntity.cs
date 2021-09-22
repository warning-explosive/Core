namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using DataAccess.Api.DatabaseEntity;

    internal class CapturedMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public CapturedMessageDatabaseEntity(
            Guid primaryKey,
            IntegrationMessageDatabaseEntity message,
            string? refuseReason)
            : base(primaryKey)
        {
            Message = message;
            RefuseReason = refuseReason;
        }

        public IntegrationMessageDatabaseEntity Message { get; }

        public string? RefuseReason { get; }
    }
}