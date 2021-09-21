namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using DataAccess.Api.DatabaseEntity;

    internal class IntegrationMessageHeaderDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageHeaderDatabaseEntity(Guid primaryKey, JsonObject value)
            : base(primaryKey)
        {
            Value = value;
        }

        public JsonObject Value { get; }
    }
}