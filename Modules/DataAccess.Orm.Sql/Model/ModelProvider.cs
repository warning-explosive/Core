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

        private ModelInfo? _model;

        public ModelProvider(
            IDependencyContainer dependencyContainer,
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> Objects
        {
            get
            {
                _model ??= InitModel();
                return _model.Objects;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<Type, (Type Left, Type Right)>> MtmTables
        {
            get
            {
                _model ??= InitModel();
                return _model.MtmTables;
            }
        }

        private ModelInfo InitModel()
        {
            var mutableMtmTables =
                new Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>(StringComparer.OrdinalIgnoreCase);

            var objects = _databaseTypeProvider.DatabaseEntities()
                .SelectMany(entity => GetEntityInfos(entity, mutableMtmTables))
                .Distinct()
                .GroupBy(info => info.Schema, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.ToDictionary(info => info.Type.Name, StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IObjectModelInfo>,
                    StringComparer.OrdinalIgnoreCase);

            var mtmTables = mutableMtmTables
                .ToDictionary(
                    schema => schema.Key,
                    schema => schema.Value as IReadOnlyDictionary<Type, (Type, Type)>);

            return new ModelInfo(objects, mtmTables);
        }

        private IEnumerable<IObjectModelInfo> GetEntityInfos(
            Type entity,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            if (entity.IsSqlView())
            {
                yield return GetViewInfo(entity.SchemaName(), entity, mtmTables);
            }
            else
            {
                var tableInfo = GetTableInfo(entity.SchemaName(), entity, mtmTables);
                yield return tableInfo;

                foreach (var mtmTableInfo in GetMtmTablesInfo(tableInfo.Columns.Values, mtmTables))
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private TableInfo GetTableInfo(
            string schema,
            Type tableType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(schema, tableType, property, mtmTables))
                .ToList();

            return new TableInfo(schema, tableType, columns);
        }

        private IEnumerable<TableInfo> GetMtmTablesInfo(
            IEnumerable<ColumnInfo> columns,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            return columns
                .Where(column => column.IsMultipleRelation)
                .Select(GetMtmTableInfo);

            TableInfo GetMtmTableInfo(ColumnInfo columnInfo)
            {
                return GetTableInfo(columnInfo.Schema, columnInfo.Property.ReflectedType, mtmTables);
            }
        }

        private ViewInfo GetViewInfo(
            string schema,
            Type viewType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var query = viewType.SqlViewQuery(_dependencyContainer);

            var columns = viewType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .SelectMany(property => GetColumnInfo(schema, viewType, property, mtmTables))
                .ToList();

            return new ViewInfo(schema, viewType, columns, query);
        }

        private IEnumerable<ColumnInfo> GetColumnInfo(
            string schema,
            Type table,
            PropertyInfo property,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            if (!property.PropertyType.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property, mtmTables))
            {
                yield return new ColumnInfo(schema, table, chain, this);
            }
        }

        private IEnumerable<PropertyInfo[]> FlattenSpecialTypes(
            PropertyInfo property,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            if (typeof(IInlinedObject).IsAssignableFrom(property.PropertyType))
            {
                return FlattenInlinedObject(property, mtmTables);
            }

            if (property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                return FlattenRelation(property);
            }

            if (property.PropertyType.IsMultipleRelation(out var itemType))
            {
                return FlattenArrayOrMultipleRelation(property, itemType, mtmTables);
            }

            return new[]
            {
                new[]
                {
                    property
                }
            };
        }

        private IEnumerable<PropertyInfo[]> FlattenInlinedObject(
            PropertyInfo property,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var properties = property
                .PropertyType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

            foreach (var inlined in properties)
            {
                foreach (var subsequent in FlattenSpecialTypes(inlined, mtmTables))
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

        private IEnumerable<PropertyInfo[]> FlattenArrayOrMultipleRelation(
            PropertyInfo property,
            Type itemType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            return itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                ? FlattenMultipleRelation(property, itemType, mtmTables)
                : FlattenArray(property, itemType);
        }

        private IEnumerable<PropertyInfo[]> FlattenMultipleRelation(
            PropertyInfo property,
            Type itemType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var left = property.DeclaringType;
            var right = itemType;

            PropertyInfo relationProperty;

            if (TryGetMtmType(mtmTables, left, right, out var mtm))
            {
                relationProperty = mtm.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));
            }
            else if (TryGetMtmType(mtmTables, right, left, out mtm))
            {
                relationProperty = mtm.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right));
            }
            else
            {
                mtm = BuildMtmType(mtmTables, left, right);
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

        private static bool TryGetMtmType(
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables,
            Type left,
            Type right,
            [NotNullWhen(true)] out Type? mtm)
        {
            var schema = DatabaseModelExtensions.MtmSchemaName(left, right);

            if (!mtmTables.TryGetValue(schema, out var schemaGroup))
            {
                mtm = null;
                return false;
            }

            mtm = schemaGroup
                .SingleOrDefault(pair => pair.Value.Left == left && pair.Value.Right == right)
                .Key;

            return mtm != null;
        }

        private Type BuildMtmType(
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables,
            Type left,
            Type right)
        {
            var schema = DatabaseModelExtensions.MtmSchemaName(left, right);

            var leftKey = left.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();
            var rightKey = right.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();

            var mtmBaseType = typeof(BaseMtmDatabaseEntity<,>).MakeGenericType(leftKey, rightKey);

            var mtmTypeName = string.Join("_", left.Name, right.Name);

            var dynamicClass = new DynamicClass(mtmTypeName).InheritsFrom(mtmBaseType);
            var mtmType = _dynamicClassProvider.CreateType(dynamicClass);

            var schemaGroup = mtmTables.GetOrAdd(schema, _ => new Dictionary<Type, (Type Left, Type Right)>());
            schemaGroup[mtmType] = (left, right);

            return mtmType;
        }

        private class ModelInfo
        {
            public ModelInfo(
                IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> objects,
                IReadOnlyDictionary<string, IReadOnlyDictionary<Type, (Type, Type)>> mtmTables)
            {
                Objects = objects;
                MtmTables = mtmTables;
            }

            public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> Objects { get; }

            public IReadOnlyDictionary<string, IReadOnlyDictionary<Type, (Type, Type)>> MtmTables { get; }
        }
    }
}