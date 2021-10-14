namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Dynamic;
    using Dynamic.Abstractions;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelProvider : IModelProvider
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDynamicClassProvider _dynamicClassProvider;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>>? _model;
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>>? _tableTypes;

        public ModelProvider(
            IDependencyContainer dependencyContainer,
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> Model
        {
            get
            {
                _model ??= InitModel();
                return _model;
            }
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TableTypes
        {
            get
            {
                _tableTypes ??= InitTableTypes();
                return _tableTypes;
            }
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> InitTableTypes()
        {
            return _databaseTypeProvider
                .DatabaseEntities()
                .Where(entity => !entity.IsSqlView())
                .GroupBy(table => table.SchemaName())
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.ToDictionary(
                        table => table.Name,
                        StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, Type>,
                    StringComparer.OrdinalIgnoreCase);
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> InitModel()
        {
            var model = new Dictionary<string, Dictionary<string, IObjectModelInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var info in _databaseTypeProvider.DatabaseEntities().SelectMany(GetEntityInfos))
            {
                model
                    .GetOrAdd(info.Schema, _ => new Dictionary<string, IObjectModelInfo>(StringComparer.OrdinalIgnoreCase))
                    .Add(info.Type.Name, info);
            }

            return model
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value as IReadOnlyDictionary<string, IObjectModelInfo>,
                    StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<IObjectModelInfo> GetEntityInfos(Type entity)
        {
            if (entity.IsSqlView())
            {
                yield return GetViewInfo(entity);
            }
            else
            {
                var tableInfo = GetTableInfo(entity);
                yield return tableInfo;

                foreach (var mtmTableInfo in GetMtmTablesInfo(tableInfo.Columns.Values))
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private TableInfo GetTableInfo(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(tableType, property))
                .ToList();

            return new TableInfo(tableType, columns);
        }

        private IEnumerable<TableInfo> GetMtmTablesInfo(IEnumerable<ColumnInfo> columns)
        {
            return columns
                .Where(column => column.IsMultipleRelation)
                .Select(GetMtmTableInfo);

            TableInfo GetMtmTableInfo(ColumnInfo columnInfo)
            {
                return GetTableInfo(columnInfo.Property.ReflectedType);
            }
        }

        private ViewInfo GetViewInfo(Type viewType)
        {
            var query = viewType.SqlViewQuery(_dependencyContainer);

            var columns = viewType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(viewType, property))
                .ToList();

            return new ViewInfo(viewType, columns, query);
        }

        private IEnumerable<ColumnInfo> GetColumnInfo(Type table, PropertyInfo property)
        {
            if (!property.PropertyType.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property))
            {
                yield return new ColumnInfo(table, chain, TableTypes);
            }
        }

        private IEnumerable<PropertyInfo[]> FlattenSpecialTypes(PropertyInfo property)
        {
            if (typeof(IInlinedObject).IsAssignableFrom(property.PropertyType))
            {
                return FlattenInlinedObject(property);
            }

            if (property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                return FlattenRelation(property);
            }

            if (property.PropertyType.IsMultipleRelation(out var itemType))
            {
                return FlattenArrayOrMultipleRelation(property, itemType);
            }

            return new[]
            {
                new[]
                {
                    property
                }
            };
        }

        private IEnumerable<PropertyInfo[]> FlattenInlinedObject(PropertyInfo property)
        {
            var properties = property
                .PropertyType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

            foreach (var inlined in properties)
            {
                foreach (var subsequent in FlattenSpecialTypes(inlined))
                {
                    yield return new[] { property }
                        .Concat(subsequent)
                        .ToArray();
                }
            }
        }

        private static IEnumerable<PropertyInfo[]> FlattenRelation(PropertyInfo property)
        {
            var primaryKeyProperty = property
                .PropertyType
                .GetProperty(nameof(IUniqueIdentified<Guid>.PrimaryKey));

            yield return new[]
            {
                property,
                primaryKeyProperty
            };
        }

        private IEnumerable<PropertyInfo[]> FlattenArrayOrMultipleRelation(PropertyInfo property, Type itemType)
        {
            return itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                ? FlattenMultipleRelation(property, itemType)
                : FlattenArray(property, itemType);
        }

        private IEnumerable<PropertyInfo[]> FlattenMultipleRelation(PropertyInfo property, Type itemType)
        {
            var left = property.DeclaringType;
            var leftKey = left.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();
            var right = itemType;
            var rightKey = right.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();

            var mtmType = typeof(BaseMtmDatabaseEntity<,>).MakeGenericType(leftKey, rightKey);

            var mtmTypeName = string.Join(
                "_",
                left.SchemaName(),
                left.Name,
                property.Name,
                right.SchemaName(),
                right.Name);

            var dynamicClass = new DynamicClass(mtmTypeName).InheritsFrom(mtmType);

            var relationProperty = _dynamicClassProvider
                .CreateType(dynamicClass)
                .GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));

            return new[]
            {
                new[]
                {
                    property,
                    relationProperty
                }
            };
        }

        private static IEnumerable<PropertyInfo[]> FlattenArray(PropertyInfo property, Type itemType)
        {
            throw new NotSupportedException($"Arrays are not supported: {property.Name} - {itemType.Name}[]");
        }
    }
}