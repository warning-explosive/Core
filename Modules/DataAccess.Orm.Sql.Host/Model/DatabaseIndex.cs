namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
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

        public string Schema { get; set; }

        public string Table { get; set; }

        public string Index { get; set; }

        public string Definition { get; set; }
    }
}