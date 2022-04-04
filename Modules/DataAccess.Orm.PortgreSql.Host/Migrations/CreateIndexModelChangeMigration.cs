namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Host.Model;
    using Sql.Host.Migrations;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexModelChangeMigration : IModelChangeMigration<CreateIndex>
    {
        private const string CommandFormat = @"create {4}index ""{2}"" on ""{0}"".""{1}"" ({3})";

        private readonly IModelProvider _modelProvider;

        public CreateIndexModelChangeMigration(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public Task<string> Migrate(CreateIndex change, CancellationToken token)
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

            return Task.FromResult(commandText);
        }
    }
}