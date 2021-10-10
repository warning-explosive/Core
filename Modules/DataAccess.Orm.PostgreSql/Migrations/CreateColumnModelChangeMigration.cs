namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateColumnModelChangeMigration : IModelChangeMigration<CreateColumn>
    {
        private const string CommandFormat = @"alter table ""{0}"".""{1}"" add ""{2}"" {3}{4}";

        private readonly IModelProvider _modelProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CreateColumnModelChangeMigration(
            IModelProvider modelProvider,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            _modelProvider = modelProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public Task<string> Migrate(CreateColumn change, CancellationToken token)
        {
            if (!_modelProvider.Model.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || !info.Columns.TryGetValue(change.Column, out var column))
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table}.{change.Column} isn't presented in the model");
            }

            var (columnName, dataType, modifiers) = CreateColumn(column);

            string constraints;

            if (modifiers.Any())
            {
                var separator = " ";
                constraints = separator + modifiers.ToString(separator);
            }
            else
            {
                constraints = string.Empty;
            }

            var command = CommandFormat.Format(change.Schema, change.Table, columnName, dataType, constraints);

            return Task.FromResult(command);
        }

        internal (string Column, string DataType, IReadOnlyCollection<string> Constraints) CreateColumn(ColumnInfo column)
        {
            var dataType = _columnDataTypeProvider.GetColumnDataType(column.Type);

            return (column.Name, dataType, column.Constraints);
        }
    }
}