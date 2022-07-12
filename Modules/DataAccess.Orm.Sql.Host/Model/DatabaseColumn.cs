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

        public string Schema { get; init; }

        public string Table { get; init; }

        public string Column { get; init; }

        public int Position { get; init; }

        public string DataType { get; init; }

        public bool Nullable { get; init; }

        public string? DefaultValue { get; init; }

        public int? Scale { get; init; }

        public int? Precision { get; init; }

        public int? Length { get; init; }
    }
}