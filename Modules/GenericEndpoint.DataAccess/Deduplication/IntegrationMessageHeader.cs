namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CrossCuttingConcerns.Json;
    using Messaging.Abstractions;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
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