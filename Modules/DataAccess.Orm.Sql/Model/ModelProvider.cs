namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Dynamic;
    using Dynamic.Abstractions;
    using Extensions;
    using Orm.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class ModelProvider : IModelProvider,
                                   IResolvable<IModelProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDynamicClassProvider _dynamicClassProvider;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;

        private readonly ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>> _columnsCache;
        private readonly Dictionary<Type, (Type Left, Type Right)> _mtmTablesCache;

        private readonly object _sync;
        private bool _tablesBuilt;
        private bool _mtmTablesBuilt;

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>>? _tablesMap;
        private IReadOnlyDictionary<Type, ITableInfo>? _tables;
        private IReadOnlyDictionary<Type, MtmTableInfo>? _mtmTables;

        public ModelProvider(
            IDependencyContainer dependencyContainer,
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;

            _sync = new object();
            _tablesBuilt = false;
            _mtmTablesBuilt = false;

            _columnsCache = new ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>>();
            _mtmTablesCache = new Dictionary<Type, (Type Left, Type Right)>();
        }

        public IReadOnlyDictionary<Type, ITableInfo> Tables
        {
            get
            {
                _tables ??= InitTables();

                return _tables;
            }
        }

        public IReadOnlyDictionary<Type, MtmTableInfo> MtmTables
        {
            get
            {
                _tables ??= InitTables();
                _mtmTables ??= InitMtmTables();

                return _mtmTables;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap
        {
            get
            {
                _tables ??= InitTables();
                _mtmTables ??= InitMtmTables();
                _tablesMap ??= InitTablesMap();

                return _tablesMap;
            }
        }

        private bool Locked => _tablesBuilt && _mtmTablesBuilt;

        public IEnumerable<ITableInfo> TablesFor(IReadOnlyCollection<Type> databaseEntities)
        {
            databaseEntities = _databaseTypeProvider
                .DatabaseEntities()
                .Intersect(databaseEntities)
                .ToArray();

            foreach (var (_, info) in Tables)
            {
                if (databaseEntities.Contains(info.Type)
                    || (info is MtmTableInfo mtmTableInfo
                        && mtmTableInfo.Columns.Any(column => databaseEntities.Contains(column.Value.Relation.Target))))
                {
                    yield return info;
                }
            }
        }

        public IEnumerable<ColumnInfo> Columns(Type type)
        {
            return Tables.TryGetValue(type, out var info)
                ? info.Columns.Values
                : _columnsCache.GetOrAdd(type, ValueFactory);

            IReadOnlyCollection<ColumnInfo> ValueFactory(Type table)
            {
                return new TableInfo(table, this)
                    .Columns
                    .Values
                    .ToArray();
            }
        }

        public IEnumerable<ColumnInfo> Columns(ITableInfo table)
        {
            return table
                .Type
                .Columns()
                .SelectMany(property => GetColumns(table, property))
                .ToList();
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Schema name</returns>
        public string TableName(Type type)
        {
            return type.Name;
        }

        /// <summary>
        /// Gets schema name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Schema name</returns>
        public string SchemaName(Type type)
        {
            if (type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
            {
                if (_mtmTables?.TryGetValue(type, out var mtmTableInfo) == true)
                {
                    return MtmSchemaName(mtmTableInfo.Left, mtmTableInfo.Right);
                }

                throw new InvalidOperationException($"For Mtm tables use {nameof(MtmSchemaName)} method");
            }

            return type.GetRequiredAttribute<SchemaAttribute>().Schema;
        }

        private string MtmSchemaName(Type left, Type right)
        {
            return new[] { SchemaName(left), SchemaName(right) }
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToString("_");
        }

        private IReadOnlyDictionary<Type, ITableInfo> InitTables()
        {
            lock (_sync)
            {
                if (_tablesBuilt)
                {
                    return _tables ?? throw new InvalidOperationException("Model should be initialized");
                }

                var tables = _databaseTypeProvider
                   .DatabaseEntities()
                   .SelectMany(GetTableInfo)
                   .Distinct()
                   .ToDictionary(info => info.Type);

                _tablesBuilt = true;

                return tables;
            }

            IEnumerable<ITableInfo> GetTableInfo(Type entity)
            {
                ITableInfo info = entity.IsSqlView()
                    ? new ViewInfo(entity, entity.SqlViewQuery(_dependencyContainer), this)
                    : new TableInfo(entity, this);

                yield return info;

                var mtmTableInfos = info
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .Select(column => new MtmTableInfo(column.MultipleRelationTable!, column.Relation.Source, column.Relation.Target, this));

                foreach (var mtmTableInfo in mtmTableInfos)
                {
                    yield return mtmTableInfo;
                }
            }
        }

        private IReadOnlyDictionary<Type, MtmTableInfo> InitMtmTables()
        {
            lock (_sync)
            {
                if (_mtmTablesBuilt)
                {
                    return _mtmTables ?? throw new InvalidOperationException("Model should be initialized");
                }

                var mtmTables = _tables
                   .Values
                   .OfType<MtmTableInfo>()
                   .ToDictionary(mtmTable => mtmTable.Type);

                _mtmTablesBuilt = true;

                return mtmTables;
            }
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> InitTablesMap()
        {
            lock (_sync)
            {
                if (!_tablesBuilt)
                {
                    throw new InvalidOperationException("Model should be initialized");
                }

                return _tables
                   .GroupBy(info => info.Value.Schema)
                   .ToDictionary(grp => grp.Key,
                        grp => grp.ToDictionary(info => info.Value.Name,
                            info => info.Value) as IReadOnlyDictionary<string, ITableInfo>,
                        StringComparer.OrdinalIgnoreCase);
            }
        }

        private IEnumerable<ColumnInfo> GetColumns(ITableInfo table, ColumnProperty property)
        {
            if (!property.PropertyType.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {property.Name} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property))
            {
                yield return new ColumnInfo(table, chain, this);
            }
        }

        private IEnumerable<ColumnProperty[]> FlattenSpecialTypes(ColumnProperty property)
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
                return FlattenMultipleRelation(property, itemType);
            }

            return new[]
            {
                new[]
                {
                    property
                }
            };
        }

        private IEnumerable<ColumnProperty[]> FlattenInlinedObject(ColumnProperty property)
        {
            foreach (var inlined in property.PropertyType.Columns())
            {
                foreach (var subsequent in FlattenSpecialTypes(inlined))
                {
                    yield return new[] { property }
                        .Concat(subsequent)
                        .ToArray();
                }
            }
        }

        private static IEnumerable<ColumnProperty[]> FlattenRelation(ColumnProperty property)
        {
            var primaryKeyProperty = property
                .PropertyType
                .Column(nameof(IUniqueIdentified.PrimaryKey));

            yield return new[]
            {
                property,
                primaryKeyProperty
            };
        }

        private IEnumerable<ColumnProperty[]> FlattenMultipleRelation(ColumnProperty property, Type itemType)
        {
            if (!property.ReflectedType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                throw new NotSupportedException($"{property.ReflectedType.Name} should implement {typeof(IUniqueIdentified<>)} so as to have multiple relation {property.Name} to {itemType.Name}");
            }

            if (!itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                throw new NotSupportedException($"{itemType.Name} should implement {typeof(IUniqueIdentified<>)} so as to be used as item type for {property.Name} multiple relation");
            }

            var left = property.DeclaringType;
            var right = itemType;

            ColumnProperty relationProperty;

            if (TryGetMtmType(left, right, out var mtm))
            {
                relationProperty = mtm.Column(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));
            }
            else if (TryGetMtmType(right, left, out mtm))
            {
                relationProperty = mtm.Column(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right));
            }
            else if (Locked)
            {
                throw new InvalidOperationException($"Unable to find multiple relation {property.Name} from {property.ReflectedType.Name} to {itemType.Name}");
            }
            else
            {
                mtm = BuildMtmType(left, right);
                relationProperty = mtm.Column(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left));
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

        private bool TryGetMtmType(
            Type left,
            Type right,
            [NotNullWhen(true)] out Type? mtm)
        {
            mtm = _mtmTablesCache
                .SingleOrDefault(pair => pair.Value.Left == left && pair.Value.Right == right)
                .Key;

            return mtm != null;
        }

        private Type BuildMtmType(Type left, Type right)
        {
            var leftKey = left.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();
            var rightKey = right.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single();

            var mtmBaseType = typeof(BaseMtmDatabaseEntity<,>).MakeGenericType(leftKey, rightKey);

            var schema = MtmSchemaName(left, right);
            var mtmTypeName = string.Join("_", TableName(left), TableName(right));

            var dynamicClass = new DynamicClass(schema, mtmTypeName).InheritsFrom(mtmBaseType);
            var mtmType = _dynamicClassProvider.CreateType(dynamicClass);

            _mtmTablesCache.Add(mtmType, (left, right));

            return mtmType;
        }
    }
}