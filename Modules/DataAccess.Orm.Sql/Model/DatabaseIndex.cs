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

        public Guid PrimaryKey { get; }

        public string Schema { get; }

        public string Table { get; }

        public string Index { get; }

        public string Definition { get; }
    }
}