﻿namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;
    using Sql.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateIndex>,
                                                          IResolvable<IModelChangeCommandBuilder<CreateIndex>>,
                                                          ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create {0}index ""{1}"" on ""{2}"".""{3}"" ({4}){5}{6}";
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