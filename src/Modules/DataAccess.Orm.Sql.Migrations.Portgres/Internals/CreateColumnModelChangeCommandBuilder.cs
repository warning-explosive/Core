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
    internal class CreateColumnModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateColumn>,
                                                           IResolvable<CreateColumnModelChangeCommandBuilder>,
                                                           IResolvable<IModelChangeCommandBuilder<CreateColumn>>,
                                                           ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"alter table ""{0}"".""{1}"" add ""{2}"" {3}{4}";

        private readonly IModelProvider _modelProvider;

        public CreateColumnModelChangeCommandBuilder(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
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

            var commandText = CommandFormat.Format(
                change.Schema,
                change.Table,
                column.Name,
                column.DataType,
                column.Constraints.Any() ? " " + column.Constraints.ToString(" ") : string.Empty);

            yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
        }
    }
}