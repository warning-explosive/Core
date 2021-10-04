namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;
    using Views;

    [Index(nameof(Schema), nameof(Table), nameof(Column), Unique = true)]
    internal class DatabaseColumn : ISqlView<Guid>
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
        {
            PrimaryKey = primaryKey;
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

        public Guid PrimaryKey { get; private set; }

        public string Schema { get; private set; }

        public string Table { get; private set; }

        public string Column { get; private set; }

        public int Position { get; private set; }

        public string DataType { get; private set; }

        public bool Nullable { get; private set; }

        public string? DefaultValue { get; private set; }

        public int? Scale { get; private set; }

        public int? Precision { get; private set; }

        public int? Length { get; private set; }
    }
}