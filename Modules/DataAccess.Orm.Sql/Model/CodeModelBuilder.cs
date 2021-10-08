namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
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

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CodeModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider databaseTypeProvider,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _databaseTypeProvider = databaseTypeProvider;
            _connectionProvider = connectionProvider;
        }

        public Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var schemas = _databaseTypeProvider
                .DatabaseEntities()
                .GroupBy(entity => entity.SchemaName())
                .Select(grp => BuildSchemaNode(grp.Key, grp))
                .ToArray();

            return Task.FromResult((DatabaseNode?)new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas));
        }

        private SchemaNode BuildSchemaNode(string schema, IEnumerable<Type> entities)
        {
            var tables = new List<TableNode>();
            var views = new List<ViewNode>();
            var indexes = new List<IndexNode>();

            foreach (var entity in entities)
            {
                if (entity.IsSqlView())
                {
                    views.Add(BuildViewNode(schema, entity));
                }
                else
                {
                    tables.Add(BuildTableNode(schema, entity));
                }

                indexes.AddRange(BuildIndexNodes(schema, entity));
            }

            return new SchemaNode(schema, tables, views, indexes);
        }

        // TODO: #110 - create model cache
        private static TableNode BuildTableNode(string schema, Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Select(Validate)
                .SelectMany(FlattenSpecialTypes)
                .Select(BuildColumn)
                .Select(column => new ColumnNode(schema, tableType.Name, column.Name, column.Type))
                .ToList();

            return new TableNode(schema, tableType.Name, tableType, columns);

            static PropertyInfo Validate(PropertyInfo property)
            {
                if (!property.PropertyType.IsTypeSupported())
                {
                    throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
                }

                return property;
            }

            static IEnumerable<(string Name, Type Type)[]> FlattenSpecialTypes(PropertyInfo property)
            {
                if (typeof(IInlinedObject).IsAssignableFrom(property.PropertyType))
                {
                    return FlattenInlinedObject(property);
                }

                if (property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
                {
                    return FlattenRelation(property);
                }

                if (property.PropertyType.IsSubclassOfOpenGeneric(typeof(IReadOnlyCollection<>)))
                {
                    var itemType = property
                        .PropertyType
                        .UnwrapTypeParameter(typeof(IReadOnlyCollection<>));

                    return itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                        ? FlattenMultipleRelation(property, itemType)
                        : FlattenArray(property, itemType);
                }

                return new[]
                {
                    new[]
                    {
                        (property.Name, property.PropertyType)
                    }
                };
            }

            static IEnumerable<(string Name, Type Type)[]> FlattenInlinedObject(PropertyInfo property)
            {
                var properties = property
                    .PropertyType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

                foreach (var inlined in properties)
                {
                    foreach (var subsequent in FlattenSpecialTypes(inlined))
                    {
                        yield return new[] { (property.Name, property.PropertyType) }
                            .Concat(subsequent)
                            .ToArray();
                    }
                }
            }

            static IEnumerable<(string Name, Type Type)[]> FlattenRelation(PropertyInfo property)
            {
                var primaryKeyType = property
                    .PropertyType
                    .ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>))
                    .Single();

                yield return new[]
                {
                    (property.Name, property.PropertyType),
                    (nameof(IUniqueIdentified<Guid>.PrimaryKey), primaryKeyType)
                };
            }

            static IEnumerable<(string Name, Type Type)[]> FlattenMultipleRelation(PropertyInfo property, Type itemType)
            {
                var primaryKeyType = itemType
                    .ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>))
                    .Single();

                return new[]
                {
                    new[]
                    {
                        (property.Name, primaryKeyType)
                    }
                };
            }

            static IEnumerable<(string Name, Type Type)[]> FlattenArray(PropertyInfo property, Type itemType)
            {
                throw new NotSupportedException($"Arrays are not supported: {property.Name} - {itemType.Name}[]");
            }

            static (string Name, Type Type) BuildColumn((string Name, Type Type)[] properties)
            {
                var name = properties
                    .Select(property => property.Name)
                    .ToString("_");

                var type = properties
                    .Last()
                    .Type;

                return (name, type);
            }
        }

        private ViewNode BuildViewNode(string schema, Type viewType)
        {
            return new ViewNode(schema, viewType.Name, viewType.SqlViewQuery(_dependencyContainer));
        }

        private static IEnumerable<IndexNode> BuildIndexNodes(string schema, Type entity)
        {
            return entity
                .GetAttributes<IndexAttribute>()
                .Select(index => new IndexNode(schema, entity.Name, index.Columns, index.Unique));
        }
    }
}