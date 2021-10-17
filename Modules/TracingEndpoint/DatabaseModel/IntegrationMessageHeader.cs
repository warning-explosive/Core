namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Model;
    using GenericEndpoint.Messaging.Abstractions;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record IntegrationMessageHeader : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessageHeader(Guid primaryKey, JsonObject value)
            : base(primaryKey)
        {
            Value = value;
        }

        public JsonObject Value { get; private init; }

        public IIntegrationMessageHeader BuildIntegrationMessageHeader(IJsonSerializer serializer)
        {
            return (IIntegrationMessageHeader)serializer.DeserializeObject(Value.Value, Value.SystemType);
        }

        public static IntegrationMessageHeader Build(IIntegrationMessageHeader messageHeader, IJsonSerializer serializer)
        {
            var header = new JsonObject(serializer.SerializeObject(messageHeader), messageHeader.GetType());
            return new IntegrationMessageHeader(Guid.NewGuid(), header);
        }
    }
}