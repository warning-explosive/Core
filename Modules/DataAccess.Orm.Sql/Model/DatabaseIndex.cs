namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;
    using Views;

    [Index(nameof(Schema), nameof(Table), nameof(Index), Unique = true)]
    internal class DatabaseIndex : ISqlView<Guid>
    {
        public DatabaseIndex(
            Guid primaryKey,
            string schema,
            string table,
            string index,
            string definition)
        {
            PrimaryKey = primaryKey;
            Schema = schema;
            Table = table;
            Index = index;
            Definition = definition;
        }

        public Guid PrimaryKey { get; private init; }

        public string Schema { get; private init; }

        public string Table { get; private init; }

        public string Index { get; private init; }

        public string Definition { get; private init; }
    }
}