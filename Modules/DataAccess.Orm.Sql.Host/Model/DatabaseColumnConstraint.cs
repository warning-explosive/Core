namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    /// <summary>
    /// DatabaseColumnConstraint
    /// </summary>
    [Schema(nameof(DataAccess.Orm.Sql.Host.Migrations))]
    public record DatabaseColumnConstraint : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        /// <param name="constraintType">ConstraintType</param>
        /// <param name="constraintName">ConstraintName</param>
        /// <param name="foreignSchema">ForeignSchema</param>
        /// <param name="foreignTable">ForeignTable</param>
        /// <param name="foreignColumn">ForeignColumn</param>
        public DatabaseColumnConstraint(
            Guid primaryKey,
            string schema,
            string table,
            string column,
            EnColumnConstraintType constraintType,
            string constraintName,
            string foreignSchema,
            string foreignTable,
            string foreignColumn)
            : base(primaryKey)
        {
            Schema = schema;
            Table = table;
            Column = column;
            ConstraintType = constraintType;
            ConstraintName = constraintName;
            ForeignSchema = foreignSchema;
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
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
        /// ConstraintType
        /// </summary>
        public EnColumnConstraintType ConstraintType { get; set; }

        /// <summary>
        /// ConstraintName
        /// </summary>
        public string ConstraintName { get; set; }

        /// <summary>
        /// ForeignSchema
        /// </summary>
        public string ForeignSchema { get; set; }

        /// <summary>
        /// ForeignTable
        /// </summary>
        public string ForeignTable { get; set; }

        /// <summary>
        /// ForeignColumn
        /// </summary>
        public string ForeignColumn { get; set; }
    }
}