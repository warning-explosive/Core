namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Index(nameof(Schema), nameof(Table), nameof(Index), Unique = true)]
    internal class DatabaseIndex : BaseSqlView<Guid>
    {
        public DatabaseIndex(
            Guid primaryKey,
            string schema,
            string table,
            string index,
            string definition)
            : base(primaryKey)
        {
            Schema = schema;
            Table = table;
            Index = index;
            Definition = definition;
        }

        public string Schema { get; private init; }

        public string Table { get; private init; }

        public string Index { get; private init; }

        public string Definition { get; private init; }
    }
}