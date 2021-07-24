namespace SpaceEngineers.Core.DataAccess.Orm.Model.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericDomain;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDomainTypeProvider _domainTypeProvider;

        public CodeModelBuilder(IDomainTypeProvider domainTypeProvider)
        {
            _domainTypeProvider = domainTypeProvider;
        }

        public DatabaseNode? BuildModel()
        {
            var tables = _domainTypeProvider
                .Entities()
                .Select(BuildTableNode)
                .ToList();

            // TODO: todo_database_name
            return new DatabaseNode("todo_database_name", tables);
        }

        private TableNode BuildTableNode(Type tableType)
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

        private ColumnNode BuildColumnNode(Type tableType, PropertyInfo propertyInfo)
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