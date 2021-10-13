namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Views;

    internal class DatabaseColumnConstraint : ISqlView<Guid>
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
        {
            PrimaryKey = primaryKey;
            Schema = schema;
            Table = table;
            Column = column;
            ConstraintType = constraintType;
            ConstraintName = constraintName;
            ForeignSchema = foreignSchema;
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
        }

        public Guid PrimaryKey { get; }

        public string Schema { get; }

        public string Table { get; }

        public string Column { get; }

        public EnColumnConstraintType ConstraintType { get; }

        public string ConstraintName { get; }

        public string ForeignSchema { get; }

        public string ForeignTable { get; }

        public string ForeignColumn { get; }
    }
}