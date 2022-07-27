namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    internal class DatabaseColumnConstraint : BaseSqlView<Guid>
    {
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

        public string Schema { get; set; }

        public string Table { get; set; }

        public string Column { get; set; }

        public EnColumnConstraintType ConstraintType { get; set; }

        public string ConstraintName { get; set; }

        public string ForeignSchema { get; set; }

        public string ForeignTable { get; set; }

        public string ForeignColumn { get; set; }
    }
}