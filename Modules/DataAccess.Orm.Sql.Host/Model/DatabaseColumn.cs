namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

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

        public string Schema { get; private init; }

        public string Table { get; private init; }

        public string Column { get; private init; }

        public int Position { get; private init; }

        public string DataType { get; private init; }

        public bool Nullable { get; private init; }

        public string? DefaultValue { get; private init; }

        public int? Scale { get; private init; }

        public int? Precision { get; private init; }

        public int? Length { get; private init; }
    }
}