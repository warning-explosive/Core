﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Concurrent;
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
    using Extensions;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelProvider : IModelProvider
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDynamicClassProvider _dynamicClassProvider;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;

        private readonly ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>> _columnsCache;

        private ModelInfo? _model;

        public ModelProvider(
            IDependencyContainer dependencyContainer,
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;

            _columnsCache = new ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>>();
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

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> ObjectsFor(Type[] databaseEntities)
        {
            databaseEntities = databaseEntities
                .Where(entity => _databaseTypeProvider.DatabaseEntities().Contains(entity))
                .ToArray();

            var schemas = databaseEntities
                .Select(entity => entity.SchemaName())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var tables = databaseEntities
                .Select(entity => entity.TableName())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return Objects
                .Where(schema => schemas.Contains(schema.Key))
                .ToDictionary(
                    schema => schema.Key,
                    schema => schema
                        .Value
                        .Where(table => tables.Contains(table.Key)
                                        || (table.Value is TableInfo tableInfo
                                            && tableInfo.Type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>))
                                            && tableInfo.Columns.Any(column => databaseEntities.Contains(column.Value.Relation.Target))))
                        .ToDictionary(
                            table => table.Key,
                            table => table.Value,
                            StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IObjectModelInfo>);
        }

        public IEnumerable<ColumnInfo> Columns(Type type)
        {
            return _columnsCache.GetOrAdd(type, key => GetColumns(key.SchemaName(), key, null));
        }

        private ModelInfo InitModel()
        {
            var mutableMtmTables = new Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>(StringComparer.OrdinalIgnoreCase);

            var objects = _databaseTypeProvider
                .DatabaseEntities()
                .SelectMany(entity => GetEntityInfos(entity, mutableMtmTables))
                .Distinct()
                .GroupBy(info => info.Schema, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.ToDictionary(info => info.Type.TableName(), StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IObjectModelInfo>,
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
                yield return GetView(entity.SchemaName(), entity, mtmTables);
            }
            else
            {
                var tableInfo = GetTable(entity.SchemaName(), entity, mtmTables);
                yield return tableInfo;

                foreach (var mtmTableInfo in GetMtmTables(tableInfo.Columns.Values, mtmTables))
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private TableInfo GetTable(
            string schema,
            Type tableType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var columns = GetColumns(schema, tableType, mtmTables).ToList();

            return new TableInfo(schema, tableType, columns);
        }

        private IEnumerable<TableInfo> GetMtmTables(
            IEnumerable<ColumnInfo> columns,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            return columns
                .Where(column => column.IsMultipleRelation)
                .Select(GetMtmTableInfo);

            TableInfo GetMtmTableInfo(ColumnInfo columnInfo)
            {
                return GetTable(columnInfo.Schema, columnInfo.MultipleRelationTable!, mtmTables);
            }
        }

        private ViewInfo GetView(
            string schema,
            Type viewType,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>> mtmTables)
        {
            var query = viewType.SqlViewQuery(_dependencyContainer);

            var columns = GetColumns(schema, viewType, mtmTables);

            return new ViewInfo(schema, viewType, columns, query);
        }

        private IReadOnlyCollection<ColumnInfo> GetColumns(
            string schema,
            Type type,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>? mtmTables)
        {
            return type
                .Columns()
                .SelectMany(property => GetColumns(schema, type, property, mtmTables))
                .ToList();
        }

        private IEnumerable<ColumnInfo> GetColumns(
            string schema,
            Type table,
            PropertyInfo property,
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>? mtmTables)
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
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>? mtmTables)
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
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>? mtmTables)
        {
            foreach (var inlined in property.PropertyType.Columns())
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
            Dictionary<string, Dictionary<Type, (Type Left, Type Right)>>? mtmTables)
        {
            return itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                   && mtmTables != null
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

            var mtmTypeName = string.Join("_", left.TableName(), right.TableName());

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