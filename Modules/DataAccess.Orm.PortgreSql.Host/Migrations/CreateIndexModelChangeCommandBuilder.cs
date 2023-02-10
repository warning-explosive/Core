namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;
    using Sql.Host.Model;
    using Sql.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateIndex>,
                                                          IResolvable<IModelChangeCommandBuilder<CreateIndex>>,
                                                          ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create {4}index ""{2}"" on ""{0}"".""{1}"" ({3})";

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

            var columns = index
                .Columns
                .Select(column => $@"""{column.Name}""")
                .ToString(", ");

            var modifiers = index.Unique
                ? "unique "
                : string.Empty;

            var commandText = CommandFormat.Format(change.Schema, change.Table, change.Index, columns, modifiers);

            yield return new SqlCommand(commandText, Array.Empty<SqlCommandParameter>());
        }
    }
}