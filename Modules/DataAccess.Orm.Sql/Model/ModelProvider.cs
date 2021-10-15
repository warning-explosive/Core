namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        private readonly Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> _mtmTables;

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>>? _model;

        public ModelProvider(
            IDependencyContainer dependencyContainer,
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;

            _mtmTables = new Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>();
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> Model
        {
            get
            {
                _model ??= InitModel();
                return _model;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<Type, (Type Left, Type Right)>> MtmTables
        {
            get
            {
                _model ??= InitModel();
                return _mtmTables
                    .ToDictionary(
                        schema => schema.Key,
                        schema => schema.Value as IReadOnlyDictionary<Type, (Type, Type)>);
            }
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
                yield return GetViewInfo(entity.SchemaName(), entity);
            }
            else
            {
                var tableInfo = GetTableInfo(entity.SchemaName(), entity);
                yield return tableInfo;

                foreach (var mtmTableInfo in GetMtmTablesInfo(tableInfo.Columns.Values))
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private TableInfo GetTableInfo(string schema, Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(schema, tableType, property))
                .ToList();

            return new TableInfo(schema, tableType, columns);
        }

        private IEnumerable<TableInfo> GetMtmTablesInfo(IEnumerable<ColumnInfo> columns)
        {
            return columns
                .Where(column => column.IsMultipleRelation)
                .Select(GetMtmTableInfo);

            TableInfo GetMtmTableInfo(ColumnInfo columnInfo)
            {
                return GetTableInfo(columnInfo.Schema, columnInfo.Property.ReflectedType);
            }
        }

        private ViewInfo GetViewInfo(string schema, Type viewType)
        {
            var query = viewType.SqlViewQuery(_dependencyContainer);

            var columns = viewType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(schema, viewType, property))
                .ToList();

            return new ViewInfo(schema, viewType, columns, query);
        }

        private IEnumerable<ColumnInfo> GetColumnInfo(string schema, Type table, PropertyInfo property)
        {
            if (!property.PropertyType.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property))
            {
                yield return new ColumnInfo(schema, table, chain, this);
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
            var right = itemType;

            PropertyInfo relationProperty;

            if (TryGetMtmType(left, right, out var mtm))
            {
                relationProperty = mtm.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));
            }
            else if (TryGetMtmType(right, left, out mtm))
            {
                relationProperty = mtm.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right));
            }
            else
            {
                mtm = BuildMtmType(left, right);
                relationProperty = mtm.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));
            }

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

        private bool TryGetMtmType(Type left, Type right, [NotNullWhen(true)] out Type? mtm)
        {
            var schema = DatabaseModelExtensions.MtmSchemaName(left, right);

            if (!_mtmTables.TryGetValue(schema, out var schemaGroup))
            {
                mtm = null;
                return false;
            }

            mtm = schemaGroup
                .SingleOrDefault(pair => pair.Value.Left == left && pair.Value.Right == right)
                .Key;

            return mtm != null;
        }

        private Type BuildMtmType(Type left, Type right)
        {
            var schema = DatabaseModelExtensions.MtmSchemaName(left, right);

            var leftKey = left.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();
            var rightKey = right.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();

            var mtmBaseType = typeof(BaseMtmDatabaseEntity<,>).MakeGenericType(leftKey, rightKey);

            var mtmTypeName = string.Join("_", left.Name, right.Name);

            var dynamicClass = new DynamicClass(mtmTypeName).InheritsFrom(mtmBaseType);
            var mtmType = _dynamicClassProvider.CreateType(dynamicClass);

            var schemaGroup = _mtmTables.GetOrAdd(schema, _ => new Dictionary<Type, (Type Left, Type Right)>());
            schemaGroup[mtmType] = (left, right);

            return mtmType;
        }
    }
}