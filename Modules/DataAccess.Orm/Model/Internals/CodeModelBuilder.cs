namespace SpaceEngineers.Core.DataAccess.Orm.Model.Internals
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Connection.Abstractions;
    using GenericDomain;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private const string DatabaseKey = "Database";

        private readonly IDomainTypeProvider _domainTypeProvider;
        private readonly IConnectionFactory _connectionFactory;

        public CodeModelBuilder(IDomainTypeProvider domainTypeProvider, IConnectionFactory connectionFactory)
        {
            _domainTypeProvider = domainTypeProvider;
            _connectionFactory = connectionFactory;
        }

        public async Task<DatabaseNode?> BuildModel()
        {
            var tables = _domainTypeProvider
                .Entities()
                .Select(BuildTableNode)
                .ToList();

            var connectionStringBuilder = await _connectionFactory
                .GetConnectionString()
                .ConfigureAwait(false);

            if (connectionStringBuilder.TryGetValue(DatabaseKey, out object value)
                && value is string database)
            {
                return new DatabaseNode(database, tables);
            }

            throw new InvalidOperationException("Cannot find database name in the settings");
        }

        private static TableNode BuildTableNode(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Instance
                               | BindingFlags.Public
                               | BindingFlags.NonPublic
                               | BindingFlags.GetProperty)
                .Select(property => BuildColumnNode(tableType, property))
                .ToList();

            return new TableNode(tableType, tableType.Name, columns);
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
    }
}