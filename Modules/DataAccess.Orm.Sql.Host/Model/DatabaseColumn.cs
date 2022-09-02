namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    /// <summary>
    /// DatabaseColumn
    /// </summary>
    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Schema), nameof(Table), nameof(Column), Unique = true)]
    public record DatabaseColumn : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        /// <param name="position">Position</param>
        /// <param name="dataType">DataType</param>
        /// <param name="nullable">Nullable</param>
        /// <param name="defaultValue">DefaultValue</param>
        /// <param name="scale">Scale</param>
        /// <param name="precision">Precision</param>
        /// <param name="length">Length</param>
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

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Column
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Position
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// DataType
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Nullable
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Scale
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Precision
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        public int? Length { get; set; }
    }
}