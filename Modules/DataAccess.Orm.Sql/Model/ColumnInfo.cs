namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Basics;
    using Translation.Expressions;

    internal class ColumnInfo : IModelInfo,
                                IEquatable<ColumnInfo>,
                                ISafelyEquatable<ColumnInfo>
    {
        private readonly ColumnProperty _property;
        private readonly ColumnProperty[] _chain;

        private readonly IModelProvider _modelProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        private Type? _type;
        private string? _dataType;
        private bool? _isSupportedColumn;
        private bool? _isEnum;
        private bool? _isJsonColumn;
        private Relation? _relation;
        private bool? _isRelation;
        private bool? _isMultipleRelation;
        private Type? _multipleRelationTable;
        private IReadOnlyCollection<string>? _constraints;
        private Func<ISqlExpression, ISqlExpression>? _sqlExpressionExtractor;
        private Func<object?, object?>? _valueExtractor;
        private MethodInfo? _mtmCctor;

        public ColumnInfo(
            ITableInfo table,
            ColumnProperty property,
            ColumnProperty[] chain,
            IModelProvider modelProvider,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            Table = table;

            _property = property;
            _chain = chain;

            _modelProvider = modelProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public ITableInfo Table { get; }

        public string Name => _property.Name;

        public Type Type
        {
            get
            {
                return _type ??= InitType(_chain, IsRelation, _property);

                static Type InitType(
                    ColumnProperty[] chain,
                    bool isRelation,
                    ColumnProperty property)
                {
                    var type = chain.Last().Declared.PropertyType;

                    return isRelation
                           && property.Declared.IsNullable()
                           && !type.IsNullable()
                           && type.IsValueType
                        ? typeof(Nullable<>).MakeGenericType(type)
                        : type;
                }
            }
        }

        public string DataType
        {
            get
            {
                return _dataType ??= _columnDataTypeProvider.GetColumnDataType(this);
            }
        }

        public bool IsSupportedColumn
        {
            get
            {
                return _isSupportedColumn ??= _property.Declared.IsSupportedColumn();
            }
        }

        public bool IsEnum
        {
            get
            {
                return _isEnum ??= Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum;
            }
        }

        public bool IsJsonColumn
        {
            get
            {
                return _isJsonColumn ??= _property.Declared.IsJsonColumn();
            }
        }

        public uint? ColumnLength => _property.Declared.GetAttribute<ColumnLenghtAttribute>()?.Length;

        public Relation? Relation
        {
            get
            {
                return _relation ??= InitRelation(_modelProvider, Table, _property);

                static Relation? InitRelation(
                    IModelProvider modelProvider,
                    ITableInfo table,
                    ColumnProperty property)
                {
                    // one-to-one
                    if (property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
                    {
                        return new Relation(table.Type, property.PropertyType, property, modelProvider);
                    }

                    // one-to-one in mtm table (left/right property)
                    if (table.IsMtmTable)
                    {
                        var mtmTable = (MtmTableInfo)modelProvider.Tables[table.Type];

                        var type = property.Name.Equals(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), StringComparison.OrdinalIgnoreCase)
                            ? mtmTable.Left
                            : mtmTable.Right;

                        return new Relation(table.Type, type, property, modelProvider);
                    }

                    // one-to-many
                    if (property.PropertyType.IsMultipleRelation(out var itemType))
                    {
                        return new Relation(table.Type, itemType, property, modelProvider);
                    }

                    return default;
                }
            }
        }

        public bool IsRelation
        {
            get
            {
                return _isRelation ??= InitIsRelation(Table, _property);

                static bool InitIsRelation(ITableInfo table, ColumnProperty property)
                {
                    return (property.PropertyType.IsDatabaseEntity() || table.IsMtmTable)
                           && !property.PropertyType.IsMultipleRelation(out _);
                }
            }
        }

        public bool IsMultipleRelation
        {
            get
            {
                return _isMultipleRelation ??= InitIsMultipleRelation(_property);

                static bool InitIsMultipleRelation(ColumnProperty property)
                {
                    return property.PropertyType.IsMultipleRelation(out _);
                }
            }
        }

        [NotNullIfNotNull(nameof(IsMultipleRelation))]
        public Type? MultipleRelationTable
        {
            get
            {
                return IsMultipleRelation
                    ? _multipleRelationTable ??= InitMultipleRelationTable(_chain)
                    : null;

                static Type InitMultipleRelationTable(ColumnProperty[] chain)
                {
                    return chain.Last().ReflectedType;
                }
            }
        }

        public IReadOnlyCollection<string> Constraints
        {
            get
            {
                return _constraints ??= InitConstraints(_modelProvider, Name, _property, IsRelation, Relation)
                    .OrderBy(constraint => constraint)
                    .ToList();

                static IEnumerable<string> InitConstraints(
                    IModelProvider modelProvider,
                    string name,
                    ColumnProperty property,
                    bool isRelation,
                    Relation? relation)
                {
                    if (name.Equals(nameof(IUniqueIdentified.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "primary key";
                    }

                    if (isRelation && relation != null)
                    {
                        var onDeleteBehavior = relation.Property.Declared.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior;

                        var onDelete = onDeleteBehavior switch
                        {
                            EnOnDeleteBehavior.NoAction => "no action",
                            EnOnDeleteBehavior.Restrict => "restrict",
                            EnOnDeleteBehavior.Cascade => "cascade",
                            EnOnDeleteBehavior.SetNull => "set nul",
                            EnOnDeleteBehavior.SetDefault => "set default",
                            _ => throw new NotSupportedException(onDeleteBehavior.ToString())
                        };

                        yield return $@"references ""{modelProvider.SchemaName(relation.Target)}"".""{modelProvider.TableName(relation.Target)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete {onDelete}";
                    }

                    // we suppose that arrays are non nullable columns
                    if (!property.Declared.IsNullable() || property.PropertyType.IsArray())
                    {
                        yield return "not null";
                    }
                }
            }
        }

        #region IEquatable

        public static bool operator ==(ColumnInfo? left, ColumnInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(ColumnInfo? left, ColumnInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return _property.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(ColumnInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(ColumnInfo other)
        {
            return _property.Equals(other._property);
        }

        #endregion

        public override string ToString()
        {
            return $"{Table.Schema}.{Table.Name}.{Name} ({Constraints.ToString(", ")})";
        }

        public ColumnExpression BuildExpression(ParameterExpression parameter)
        {
            if (parameter.Type != Table.Type)
            {
                throw new InvalidOperationException($"Parameter expression should be constructed over {Table.Type.FullName} type instead of {parameter.Type.FullName}");
            }

            if (Relation != null)
            {
                return new ColumnExpression(Relation.Property.Reflected, Type, parameter);
            }

            _sqlExpressionExtractor ??= GetValueExtractor<ISqlExpression>(_chain, AggregateColumns);

            return (ColumnExpression)_sqlExpressionExtractor.Invoke(parameter);

            static ISqlExpression AggregateColumns(PropertyInfo property, ISqlExpression source)
            {
                return new ColumnExpression(property, property.PropertyType, source);
            }
        }

        public object? GetValue(IUniqueIdentified entity)
        {
            if (IsMultipleRelation)
            {
                return null;
            }

            _valueExtractor ??= GetValueExtractor<object?>(_chain, AggregateValue);

            return _valueExtractor.Invoke(entity);
        }

        public IUniqueIdentified? GetRelationValue(IUniqueIdentified entity)
        {
            if (IsRelation)
            {
                return (IUniqueIdentified)_property.Reflected.GetValue(entity);
            }

            return null;
        }

        public IEnumerable<IUniqueIdentified> GetMultipleRelationValue(IUniqueIdentified entity)
        {
            if (IsMultipleRelation)
            {
                return ((IEnumerable)_property.Reflected.GetValue(entity)).AsEnumerable<IUniqueIdentified>();
            }

            return Enumerable.Empty<IUniqueIdentified>();
        }

        public IUniqueIdentified CreateMtm(
            IUniqueIdentified left,
            IUniqueIdentified right)
        {
            if (!IsMultipleRelation)
            {
                throw new InvalidOperationException($"Column {Name} should represent a multiple relation in order to create mtm entry");
            }

            _mtmCctor ??= GetMtmCctor(MultipleRelationTable!);

            var mtmTable = (MtmTableInfo)_modelProvider.Tables[MultipleRelationTable!];

            var parameters = mtmTable.Left == Relation.Source && mtmTable.Right == Relation.Target
                ? new[] { left.PrimaryKey, right.PrimaryKey }
                : new[] { right.PrimaryKey, left.PrimaryKey };

            return (IUniqueIdentified)_mtmCctor.Invoke(null, parameters);

            static MethodInfo GetMtmCctor(Type mtmTable)
            {
                var leftKey = mtmTable.ExtractGenericArgumentAt(typeof(BaseMtmDatabaseEntity<,>), 0);
                var rightKey = mtmTable.ExtractGenericArgumentAt(typeof(BaseMtmDatabaseEntity<,>), 1);

                return (new MethodFinder(typeof(ColumnInfo),
                            nameof(CreateMtmInstance),
                            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
                        {
                            TypeArguments = new[] { mtmTable, leftKey, rightKey },
                            ArgumentTypes = new[] { leftKey, rightKey }
                        }
                        .FindMethod() ?? throw new InvalidOperationException($"Could not find {nameof(CreateMtmInstance)} method"))
                    .MakeGenericMethod(mtmTable, leftKey, rightKey);
            }
        }

        private static Func<T, T> GetValueExtractor<T>(
            IEnumerable<ColumnProperty> chain,
            Func<PropertyInfo, T, T> aggregate)
        {
            return chain
                .Select(static property => property.Reflected)
                .Select(property => new Func<T, T>(value => aggregate(property, value)))
                .Aggregate(static (acc, next) => value => next(acc(value)));
        }

        private static TMtm CreateMtmInstance<TMtm, TLeftKey, TRightKey>(
            TLeftKey leftKey,
            TRightKey rightKey)
            where TMtm : BaseMtmDatabaseEntity<TLeftKey, TRightKey>, new()
            where TLeftKey : notnull
            where TRightKey : notnull
        {
            return new TMtm
            {
                Left = leftKey,
                Right = rightKey
            };
        }

        private static object? AggregateValue(PropertyInfo property, object? value)
        {
            if (value == null)
            {
                return null;
            }

            return property.GetValue(value);
        }
    }
}