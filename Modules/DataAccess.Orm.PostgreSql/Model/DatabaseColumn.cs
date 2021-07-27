namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using Contract.Abstractions;
    using GenericDomain;

    internal class DatabaseColumn : EntityBase, IView
    {
        public DatabaseColumn(string tableName,
            string columnName,
            int position,
            string dataType,
            bool nullable,
            string defaultValue,
            int? scale,
            int? precision,
            int? length)
        {
            TableName = tableName;
            ColumnName = columnName;
            Position = position;
            DataType = dataType;
            Nullable = nullable;
            DefaultValue = defaultValue;
            Scale = scale;
            Precision = precision;
            Length = length;
        }

        public string TableName { get; private set; }

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