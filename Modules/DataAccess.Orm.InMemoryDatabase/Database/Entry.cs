namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System;

    internal record Entry
    {
        public Entry(
            object primaryKey,
            object entity,
            Type type,
            DateTime timestamp,
            EnEntryType entryType)
        {
            PrimaryKey = primaryKey;
            Entity = entity;
            Type = type;
            Timestamp = timestamp;
            EntryType = entryType;
        }

        public void Deconstruct(
            out object primaryKey,
            out object entity,
            out Type type,
            out DateTime timestamp,
            out EnEntryType entryType)
        {
            primaryKey = PrimaryKey;
            entity = Entity;
            type = Type;
            timestamp = Timestamp;
            entryType = EntryType;
        }

        public DateTime Timestamp { get; }

        public object PrimaryKey { get; }

        public object Entity { get; }

        public Type Type { get; }

        public EnEntryType EntryType { get; }
    }
}