namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Views;

    internal class DatabaseColumn : ISqlView<Guid>
    {
        public DatabaseColumn(
            Guid primaryKey,
            string tableName,
            string schema,
            string columnName,
            int position,
            string dataType,
            bool nullable,
            string defaultValue,
            int? scale,
            int? precision,
            int? length)
        {
            PrimaryKey = primaryKey;
            TableName = tableName;
            Schema = schema;
            ColumnName = columnName;
            Position = position;
            DataType = dataType;
            Nullable = nullable;
            DefaultValue = defaultValue;
            Scale = scale;
            Precision = precision;
            Length = length;
        }

        public Guid PrimaryKey { get; private set; }

        public string TableName { get; private set; }

        public string Schema { get; private set; }

        public string ColumnName { get; private set; }

        public int Position { get; private set; }

        public string DataType { get; private set; }

        public bool Nullable { get; private set; }

        public string? DefaultValue { get; private set; }

        public int? Scale { get; private set; }

        public int? Precision { get; private set; }

        public int? Length { get; private set; }
    }
}