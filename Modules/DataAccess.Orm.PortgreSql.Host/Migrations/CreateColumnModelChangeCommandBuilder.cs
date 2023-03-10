namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
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
    internal class CreateColumnModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateColumn>,
                                                           IResolvable<CreateColumnModelChangeCommandBuilder>,
                                                           IResolvable<IModelChangeCommandBuilder<CreateColumn>>,
                                                           ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"alter table ""{0}"".""{1}"" add ""{2}"" {3}{4}";

        private readonly IModelProvider _modelProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CreateColumnModelChangeCommandBuilder(
            IModelProvider modelProvider,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            _modelProvider = modelProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateColumn createColumn
                ? BuildCommands(createColumn)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateColumn change)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || !info.Columns.TryGetValue(change.Column, out var column))
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table}.{change.Column} isn't presented in the model");
            }

            if (column.IsMultipleRelation)
            {
                yield break;
            }

            var (columnName, dataType, constraints) = CreateColumn(column);

            var commandText = CommandFormat.Format(
                change.Schema,
                change.Table,
                columnName,
                dataType,
                constraints.Any() ? " " + constraints.ToString(" ") : string.Empty);

            yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
        }

        internal (string Column, string DataType, IReadOnlyCollection<string> Constraints) CreateColumn(ColumnInfo column)
        {
            var dataType = _columnDataTypeProvider.GetColumnDataType(column);

            return (column.Name, dataType, column.Constraints);
        }
    }
}