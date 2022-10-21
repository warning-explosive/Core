namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Sql.Host.Model;
    using Sql.Model;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class CreateViewModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateView>,
                                                         IResolvable<IModelChangeCommandBuilder<CreateView>>
    {
        private const string CommandFormat = $@"create materialized view ""{{0}}"".""{{1}}"" as {{2}};

insert into ""{nameof(DataAccess.Orm.Sql.Host.Migrations)}"".""{nameof(SqlView)}""(""{nameof(SqlView.PrimaryKey)}"", ""{nameof(SqlView.Version)}"", ""{nameof(SqlView.Schema)}"", ""{nameof(SqlView.View)}"", ""{nameof(SqlView.Query)}"") values ({{3}})";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IModelProvider _modelProvider;

        public CreateViewModelChangeCommandBuilder(
            IDependencyContainer dependencyContainer,
            IModelProvider modelProvider)
        {
            _dependencyContainer = dependencyContainer;
            _modelProvider = modelProvider;
        }

        public Task<string> BuildCommand(CreateView change, CancellationToken token)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.View, out var info)
                || info is not ViewInfo view)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.View} isn't presented in the model");
            }

            var insertQueryParameters = new object[]
                {
                    Guid.NewGuid().ToString(),
                    0,
                    change.Schema,
                    change.View,
                    view.Query
                }
               .QueryParameterSqlExpression(_dependencyContainer);

            var commandText = CommandFormat.Format(change.Schema, change.View, view.Query, insertQueryParameters);

            return Task.FromResult(commandText);
        }
    }
}