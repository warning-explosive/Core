namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.DatabaseEntity;
    using GenericEndpoint.Messaging.Abstractions;

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