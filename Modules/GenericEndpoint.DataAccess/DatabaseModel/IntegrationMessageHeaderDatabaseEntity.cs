namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using Core.DataAccess.Api.DatabaseEntity;
    using CrossCuttingConcerns.Api.Abstractions;
    using Messaging.Abstractions;

    internal class IntegrationMessageHeaderDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageHeaderDatabaseEntity(Guid primaryKey, JsonObject value)
            : base(primaryKey)
        {
            Value = value;
        }

        public JsonObject Value { get; }

        public IIntegrationMessageHeader BuildIntegrationMessageHeader(IJsonSerializer serializer)
        {
            return (IIntegrationMessageHeader)serializer.DeserializeObject(Value.Value, Value.Type);
        }

        public static IntegrationMessageHeaderDatabaseEntity Build(IIntegrationMessageHeader messageHeader, IJsonSerializer serializer)
        {
            var header = new JsonObject(serializer.SerializeObject(messageHeader), messageHeader.GetType());
            return new IntegrationMessageHeaderDatabaseEntity(Guid.NewGuid(), header);
        }
    }
}