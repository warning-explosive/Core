namespace SpaceEngineers.Core.DataAccess.Orm.Model.Internals
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Connection;
    using Contract.Abstractions;
    using GenericDomain;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDomainTypeProvider _domainTypeProvider;
        private readonly IConnectionFactory _connectionFactory;

        public CodeModelBuilder(
            IDependencyContainer dependencyContainer,
            IDomainTypeProvider domainTypeProvider,
            IConnectionFactory connectionFactory)
        {
            _dependencyContainer = dependencyContainer;
            _domainTypeProvider = domainTypeProvider;
            _connectionFactory = connectionFactory;
        }

        public async Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var tables = _domainTypeProvider
                .Entities()
                .Where(entity => !entity.IsView())
                .Select(BuildTableNode)
                .ToList();

            var views = (await _domainTypeProvider
                    .Entities()
                    .Where(OrmModelExtensions.IsView)
                    .Select(view => BuildViewNode(view, token))
                    .WhenAll()
                    .ConfigureAwait(false))
                .ToList();

            var database = await _connectionFactory
                .GetDatabaseName(token)
                .ConfigureAwait(false);

            return new DatabaseNode(database, tables, views);
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
            return this
                .CallMethod(nameof(BuildViewNodeGeneric))
                .WithTypeArgument(viewType)
                .WithArgument(token)
                .Invoke<Task<ViewNode>>();
        }

        private async Task<ViewNode> BuildViewNodeGeneric<TView>(CancellationToken token)
            where TView : IView
        {
            var query = await _dependencyContainer
                .Resolve<IViewQueryProvider<TView>>()
                .GetQuery(token)
                .ConfigureAwait(false);

            return new ViewNode(typeof(TView), query);
        }
    }
}