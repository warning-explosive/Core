namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateViewModelChangeMigration : IModelChangeMigration<CreateView>
    {
        private const string CommandFormat = @"create {3}view ""{0}"".""{1}"" as {2}";

        private readonly IModelProvider _modelProvider;

        public CreateViewModelChangeMigration(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public Task<string> Migrate(CreateView change, CancellationToken token)
        {
            if (!_modelProvider.Model.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.View, out var info)
                || info is not ViewInfo view)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.View} isn't presented in the model");
            }

            var modifiers = view.Materialized
                ? "materialized "
                : string.Empty;

            var command = CommandFormat.Format(change.Schema, change.View, view.Query, modifiers);

            return Task.FromResult(command);
        }
    }
}