namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Attributes;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Dynamic;
    using Dynamic.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class ModelProvider : IModelProvider,
                                   IResolvable<IModelProvider>
    {
        private readonly IDynamicClassProvider _dynamicClassProvider;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;
        private readonly ISqlViewQueryProviderComposite _sqlViewQueryProvider;

        private readonly ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>> _columnsCache;
        private readonly ConcurrentDictionary<Type, (Type Left, Type Right)> _mtmTablesCache;

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>>? _tablesMap;
        private IReadOnlyCollection<EnumTypeInfo>? _enums;
        private IReadOnlyDictionary<Type, ITableInfo>? _tables;

        public ModelProvider(
            IDynamicClassProvider dynamicClassProvider,
            IDatabaseTypeProvider databaseTypeProvider,
            IColumnDataTypeProvider columnDataTypeProvider,
            ISqlViewQueryProviderComposite sqlViewQueryProvider)
        {
            _dynamicClassProvider = dynamicClassProvider;
            _databaseTypeProvider = databaseTypeProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
            _sqlViewQueryProvider = sqlViewQueryProvider;

            _columnsCache = new ConcurrentDictionary<Type, IReadOnlyCollection<ColumnInfo>>();
            _mtmTablesCache = new ConcurrentDictionary<Type, (Type Left, Type Right)>();
        }

        public IReadOnlyCollection<EnumTypeInfo> Enums
        {
            get
            {
                _enums ??= InitEnums();
                return _enums;
            }
        }

        public IReadOnlyDictionary<Type, ITableInfo> Tables
        {
            get
            {
                _tables ??= InitTables();
                return _tables;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap
        {
            get
            {
                _tablesMap ??= InitTablesMap(Tables);
                return _tablesMap;
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

        public string TableName(Type type)
        {
            return type.Name;
        }

        public string SchemaName(Type type)
        {
            if (type.IsMtmTable())
            {
                if (_tables?.TryGetValue(type, out var tableInfo) == true
                    && tableInfo is MtmTableInfo mtmTableInfo)
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

        private IReadOnlyCollection<EnumTypeInfo> InitEnums()
        {
            return Tables
                .Values
                .SelectMany(table => table
                    .Columns
                    .Select(column => column.Value)
                    .Where(column => column.IsEnum)
                    .Select(column => new EnumTypeInfo(table.Schema, column.Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)))))
                .Distinct()
                .ToList();
        }

        private IReadOnlyDictionary<Type, ITableInfo> InitTables()
        {
            return _databaseTypeProvider
                .DatabaseEntities()
                .SelectMany(GetTableInfo)
                .Distinct()
                .ToDictionary(info => info.Type);

            IEnumerable<ITableInfo> GetTableInfo(Type entity)
            {
                ITableInfo info = entity.IsSqlView()
                    ? new ViewInfo(entity, _sqlViewQueryProvider.GetQuery(entity), this)
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

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> InitTablesMap(
            IReadOnlyDictionary<Type, ITableInfo> tables)
        {
            if (tables == null)
            {
                throw new InvalidOperationException("Model should be initialized");
            }

            return tables
                .GroupBy(info => info.Value.Schema)
                .ToDictionary(grp => grp.Key,
                    grp => grp.ToDictionary(
                        info => info.Value.Name,
                        info => info.Value) as IReadOnlyDictionary<string, ITableInfo>,
                    StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<ColumnInfo> GetColumns(ITableInfo table, ColumnProperty property)
        {
            if (!property.Declared.IsSupportedColumn())
            {
                throw new NotSupportedException($"Not supported column type: {property.Reflected} - {property.PropertyType}");
            }

            foreach (var chain in FlattenSpecialTypes(property))
            {
                yield return new ColumnInfo(table, chain, this, _columnDataTypeProvider);
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
            else if (_tables != null)
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
            var leftKey = left.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));
            var rightKey = right.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));

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