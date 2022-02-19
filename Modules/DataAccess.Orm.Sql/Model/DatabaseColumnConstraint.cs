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

        public Guid PrimaryKey { get; private init; }

        public string Schema { get; private init; }

        public string Table { get; private init; }

        public string Column { get; private init; }

        public EnColumnConstraintType ConstraintType { get; private init; }

        public string ConstraintName { get; private init; }

        public string ForeignSchema { get; private init; }

        public string ForeignTable { get; private init; }

        public string ForeignColumn { get; private init; }
    }
}