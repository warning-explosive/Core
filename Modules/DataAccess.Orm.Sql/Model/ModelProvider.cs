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
                _model ??= BuildModel();
                return _model;
            }
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> BuildModel()
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
                yield return new TableInfo(tableInfo.Type, tableInfo.Columns.Values.Where(column => column.MultipleRelation == null).ToList());

                foreach (var mtmTableInfo in GetMtmTablesInfo(tableInfo.Columns.Values.Where(column => column.MultipleRelation != null)))
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private static TableInfo GetTableInfo(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(tableType, property))
                .ToList();

            return new TableInfo(tableType, columns);
        }

        private IEnumerable<TableInfo> GetMtmTablesInfo(IEnumerable<ColumnInfo> columns)
        {
            return columns.Select(relation => GetMtmTableInfo(relation.MultipleRelation!));

            TableInfo GetMtmTableInfo(Relation relation)
            {
                var name = relation.MtmTableName();

                var keyType = relation
                    .Type
                    .ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>))
                    .Single();

                var type = typeof(BaseMtmDatabaseEntity<>).MakeGenericType(keyType);

                var dynamicClass = new DynamicClass(name).InheritsFrom(type);

                var tableType = _dynamicClassProvider.CreateType(dynamicClass);

                return GetTableInfo(tableType);
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

        private static IEnumerable<ColumnInfo> GetColumnInfo(Type table, PropertyInfo property)
        {
            if (!property.PropertyType.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property))
            {
                yield return new ColumnInfo(table, chain);
            }

            static IEnumerable<PropertyInfo[]> FlattenSpecialTypes(PropertyInfo property)
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

            static IEnumerable<PropertyInfo[]> FlattenInlinedObject(PropertyInfo property)
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

            static IEnumerable<PropertyInfo[]> FlattenRelation(PropertyInfo property)
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

            static IEnumerable<PropertyInfo[]> FlattenArrayOrMultipleRelation(PropertyInfo property, Type itemType)
            {
                return itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                    ? FlattenMultipleRelation(property, itemType)
                    : FlattenArray(property, itemType);

                static IEnumerable<PropertyInfo[]> FlattenMultipleRelation(PropertyInfo property, Type itemType)
                {
                    var primaryKeyProperty = itemType.GetProperty(nameof(IUniqueIdentified<Guid>.PrimaryKey));

                    return new[]
                    {
                        new[]
                        {
                            property,
                            primaryKeyProperty
                        }
                    };
                }

                static IEnumerable<PropertyInfo[]> FlattenArray(PropertyInfo property, Type itemType)
                {
                    throw new NotSupportedException($"Arrays are not supported: {property.Name} - {itemType.Name}[]");
                }
            }
        }
    }
}