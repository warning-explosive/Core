namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;

    internal class ComposedJsonObject
    {
        public ComposedJsonObject(string value, Guid aggregateId)
        {
            Value = value;
            AggregateId = aggregateId;
        }

        public string Value { get; init; }

        public Guid AggregateId { get; init; }
    }
}