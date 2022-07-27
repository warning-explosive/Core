namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Schema), nameof(Table), nameof(Column), Unique = true)]
    internal class DatabaseColumn : BaseSqlView<Guid>
    {
        public DatabaseColumn(
            Guid primaryKey,
            string schema,
            string table,
            string column,
            int position,
            string dataType,
            bool nullable,
            string defaultValue,
            int? scale,
            int? precision,
            int? length)
            : base(primaryKey)
        {
            Schema = schema;
            Table = table;
            Column = column;
            Position = position;
            DataType = dataType;
            Nullable = nullable;
            DefaultValue = defaultValue;
            Scale = scale;
            Precision = precision;
            Length = length;
        }

        public string Schema { get; set; }

        public string Table { get; set; }

        public string Column { get; set; }

        public int Position { get; set; }

        public string DataType { get; set; }

        public bool Nullable { get; set; }

        public string? DefaultValue { get; set; }

        public int? Scale { get; set; }

        public int? Precision { get; set; }

        public int? Length { get; set; }
    }
}