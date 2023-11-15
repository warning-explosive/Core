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
    internal class CreateTableDatabaseModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateTable>,
                                                                  IResolvable<IModelChangeCommandBuilder<CreateTable>>,
                                                                  ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create table ""{0}"".""{1}""
(
	{2}
)";

        private const string ColumnFormat = @"""{0}"" {1}{2}";

        private readonly IModelProvider _modelProvider;
        private readonly CreateColumnModelChangeCommandBuilder _createColumnCommandBuilder;

        public CreateTableDatabaseModelChangeCommandBuilder(
            IModelProvider modelProvider,
            CreateColumnModelChangeCommandBuilder createColumnCommandBuilder)
        {
            _modelProvider = modelProvider;
            _createColumnCommandBuilder = createColumnCommandBuilder;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateTable createTable
                ? BuildCommands(createTable)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateTable change)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || info is not TableInfo table)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table} isn't presented in the model");
            }

            var columns = table
                .Columns
                .Values
                .Where(column => !column.IsMultipleRelation)
                .Select(CreateColumn)
                .ToString($",{Environment.NewLine}\t");

            var commandText = CommandFormat.Format(change.Schema, change.Table, columns);

            yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
        }

        private string CreateColumn(ColumnInfo column)
        {
            return ColumnFormat.Format(
                column.Name,
                column.DataType,
                column.Constraints.Any() ? " " + column.Constraints.ToString(" ") : string.Empty);
        }
    }
}