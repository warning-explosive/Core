namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Sql;
    using Core.DataAccess.Api.Sql.Attributes;
    using CrossCuttingConcerns.Json;
    using Messaging.MessageHeaders;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    /// <summary>
    /// IntegrationMessageHeader
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    public record IntegrationMessageHeader : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="value">Value</param>
        public IntegrationMessageHeader(Guid primaryKey, JsonObject value)
            : base(primaryKey)
        {
            Value = value;
        }

        /// <summary>
        /// Value
        /// </summary>
        public JsonObject Value { get; set; }

        internal IIntegrationMessageHeader BuildIntegrationMessageHeader(IJsonSerializer serializer)
        {
            return (IIntegrationMessageHeader)serializer.DeserializeObject(Value.Value, Value.SystemType);
        }

        internal static IntegrationMessageHeader Build(IIntegrationMessageHeader messageHeader, IJsonSerializer serializer)
        {
            var header = new JsonObject(serializer.SerializeObject(messageHeader), messageHeader.GetType());
            return new IntegrationMessageHeader(Guid.NewGuid(), header);
        }
    }
}