namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using Core.DataAccess.Api;
    using Core.DataAccess.Api.Abstractions;

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