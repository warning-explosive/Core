namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateIndex>,
                                                          IResolvable<IModelChangeCommandBuilder<CreateIndex>>,
                                                          ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create {0}index ""{1}"" on ""{2}"".""{3}"" ({4}){5}{6}";
        private const string JsonIndexCommandFormat = @"create index ""{0}"" on ""{1}"".""{2}"" using gin ({3})";
        private const string ColumnFormat = @"""{0}""";

        private readonly IModelProvider _modelProvider;

        public CreateIndexModelChangeCommandBuilder(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateIndex createIndex
                ? BuildCommands(createIndex)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateIndex change)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || !info.Indexes.TryGetValue(change.Index, out var index))
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table}.{change.Index} isn't presented in the model");
            }

            if (index.Columns.Count == 1
                && index.Columns.Single() is ColumnInfo jsonColumn
                && jsonColumn.IsJsonColumn
                && index.IncludedColumns.Count == 0)
            {
                var commandText = JsonIndexCommandFormat.Format(
                    change.Index + "_gin",
                    change.Schema,
                    change.Table,
                    ColumnFormat.Format(jsonColumn.Name));

                yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
            }
            else
            {
                var unique = index.Unique
                    ? "unique "
                    : string.Empty;

                var columns = index
                    .Columns
                    .Select(column => ColumnFormat.Format(column.Name))
                    .ToString(", ");

                var includedColumns = index.IncludedColumns.Any()
                    ? " include ({0})".Format(index
                        .IncludedColumns
                        .Select(column => ColumnFormat.Format(column.Name))
                        .ToString(", "))
                    : string.Empty;

                var predicate = !index.Predicate.IsNullOrWhiteSpace()
                    ? $"where {index.Predicate}"
                    : string.Empty;

                var commandText = CommandFormat.Format(
                    unique,
                    change.Index,
                    change.Schema,
                    change.Table,
                    columns,
                    includedColumns,
                    predicate);

                yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
            }
        }
    }
}