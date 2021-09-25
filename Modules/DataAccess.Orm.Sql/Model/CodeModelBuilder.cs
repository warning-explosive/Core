namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
    using Orm.Model;
    using Views;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseTypeProvider _domainTypeProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CodeModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider domainTypeProvider,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _domainTypeProvider = domainTypeProvider;
            _connectionProvider = connectionProvider;
        }

        public async Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var tables = _domainTypeProvider
                .DatabaseEntities()
                .Where(entity => !entity.IsSqlView())
                .Select(BuildTableNode)
                .ToList();

            var views = (await _domainTypeProvider
                    .DatabaseEntities()
                    .Where(DatabaseModelExtensions.IsSqlView)
                    .Select(view => BuildViewNode(view, token))
                    .WhenAll()
                    .ConfigureAwait(false))
                .ToList();

            return new DatabaseNode(_connectionProvider.Database, tables, views);
        }

        private static TableNode BuildTableNode(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Select(property => BuildColumnNode(tableType, property))
                .ToList();

            return new TableNode(tableType, columns);
        }

        private static ColumnNode BuildColumnNode(Type tableType, PropertyInfo propertyInfo)
        {
            var tableName = tableType.Name;
            var columnType = propertyInfo.PropertyType;
            var columnName = propertyInfo.Name;

            return columnType.IsTypeSupported()
                ? new ColumnNode(columnType, columnName)
                : throw new NotSupportedException($"Not supported column type: {tableName}.{columnName} - {columnType}");
        }

        private Task<ViewNode> BuildViewNode(Type viewType, CancellationToken token)
        {
            var viewKeyType = viewType.ExtractGenericArgumentsAt(typeof(ISqlView<>)).Single();

            return this
                .CallMethod(nameof(BuildViewNodeGeneric))
                .WithTypeArgument(viewType)
                .WithTypeArgument(viewKeyType)
                .WithArgument(token)
                .Invoke<Task<ViewNode>>();
        }

        private async Task<ViewNode> BuildViewNodeGeneric<TView, TKey>(CancellationToken token)
            where TView : ISqlView<TKey>
        {
            var query = await _dependencyContainer
                .Resolve<ISqlViewQueryProvider<TView, TKey>>()
                .GetQuery(token)
                .ConfigureAwait(false);

            return new ViewNode(typeof(TView), query);
        }
    }
}