﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
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

        public string Schema { get; init; }

        public string Table { get; init; }

        public string Column { get; init; }

        public EnColumnConstraintType ConstraintType { get; init; }

        public string ConstraintName { get; init; }

        public string ForeignSchema { get; init; }

        public string ForeignTable { get; init; }

        public string ForeignColumn { get; init; }
    }
}