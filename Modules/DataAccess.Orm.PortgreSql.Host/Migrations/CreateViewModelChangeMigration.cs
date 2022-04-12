namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
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
    internal class CreateViewModelChangeMigration : IModelChangeMigration<CreateView>,
                                                    IResolvable<IModelChangeMigration<CreateView>>
    {
        private const string CommandFormat = @"create materialized view ""{0}"".""{1}"" as {2}";

        private readonly IModelProvider _modelProvider;

        public CreateViewModelChangeMigration(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public Task<string> Migrate(CreateView change, CancellationToken token)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.View, out var info)
                || info is not ViewInfo view)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.View} isn't presented in the model");
            }

            var commandText = CommandFormat.Format(change.Schema, change.View, view.Query);

            return Task.FromResult(commandText);
        }
    }
}