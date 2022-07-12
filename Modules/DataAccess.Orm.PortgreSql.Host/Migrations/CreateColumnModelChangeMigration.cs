﻿namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Host.Model;
    using Sql.Host.Migrations;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateColumnModelChangeMigration : IModelChangeMigration<CreateColumn>,
                                                      IResolvable<IModelChangeMigration<CreateColumn>>,
                                                      IResolvable<CreateColumnModelChangeMigration>
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
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || !info.Columns.TryGetValue(change.Column, out var column))
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table}.{change.Column} isn't presented in the model");
            }

            if (column.IsMultipleRelation)
            {
                return Task.FromResult(string.Empty);
            }

            var (columnName, dataType, constraints) = CreateColumn(column);

            var commandText = CommandFormat.Format(
                change.Schema,
                change.Table,
                columnName,
                dataType,
                constraints.Any() ? " " + constraints.ToString(" ") : string.Empty);

            return Task.FromResult(commandText);
        }

        internal (string Column, string DataType, IReadOnlyCollection<string> Constraints) CreateColumn(ColumnInfo column)
        {
            var dataType = _columnDataTypeProvider.GetColumnDataType(column);

            return (column.Name, dataType, column.Constraints);
        }
    }
}