namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record CapturedMessageDatabaseEntity : BaseDatabaseEntity<Guid>
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

        public IntegrationMessageDatabaseEntity Message { get; private init; }

        public string? RefuseReason { get; private init; }
    }
}