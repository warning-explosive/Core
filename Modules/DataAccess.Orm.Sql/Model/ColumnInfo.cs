namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using Api.Model;
    using Basics;
    using Extensions;
    using Translation.Expressions;

    /// <summary>
    /// ColumnInfo
    /// </summary>
    public class ColumnInfo : IModelInfo,
                              IEquatable<ColumnInfo>,
                              ISafelyEquatable<ColumnInfo>
    {
        private readonly ColumnProperty[] _chain;
        private readonly IModelProvider _modelProvider;

        private string? _name;
        private ColumnProperty? _property;
        private IReadOnlyCollection<string>? _constraints;
        private Lazy<Relation?>? _relation;
        private Lazy<bool>? _isMultipleRelation;
        private Lazy<bool>? _isInlinedObject;
        private MethodInfo? _mtmCctor;
        private Func<Expression, Expression>? _expressionExtractor;
        private Func<ISqlExpression, ISqlExpression>? _sqlExpressionExtractor;
        private Func<object?, object?>? _valueExtractor;
        private Func<object?, object?>? _relationValueExtractor;

        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        /// <param name="chain">Property chain</param>
        /// <param name="modelProvider">IModelProvider</param>
        public ColumnInfo(
            ITableInfo table,
            ColumnProperty[] chain,
            IModelProvider modelProvider)
        {
            Table = table;

            if (!chain.Any())
            {
                throw new InvalidOperationException("Column chain should contain at least one property info");
            }

            _chain = chain;

            _modelProvider = modelProvider;

            IsEnum = Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum;
        }

        /// <summary>
        /// Table type
        /// </summary>
        public ITableInfo Table { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                _name ??= InitName();
                return _name;

                string InitName()
                {
                    return _chain
                        .Select(property => property.Name)
                        .ToString("_");
                }
            }
        }

        /// <summary>
        /// Property
        /// </summary>
        public ColumnProperty Property
        {
            get
            {
                _property ??= InitProperty();
                return _property;

                ColumnProperty InitProperty()
                {
                    return _chain.Last();
                }
            }
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type => Property.PropertyType;

        /// <summary>
        /// IsEnum
        /// </summary>
        public bool IsEnum { get; }

        /// <summary>
        /// Relation
        /// </summary>
        public Relation? Relation
        {
            get
            {
                _relation ??= new Lazy<Relation?>(InitRelation, LazyThreadSafetyMode.ExecutionAndPublication);
                return _relation.Value;

                Relation? InitRelation()
                {
                    var oneToOne = _chain
                        .Reverse()
                        .FirstOrDefault(property => property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)));

                    if (oneToOne != null)
                    {
                        return new Relation(Table.Type, oneToOne.PropertyType, oneToOne, _modelProvider);
                    }

                    var oneToMany = _chain
                        .Reverse()
                        .SkipWhile(property => property.ReflectedType.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                        .FirstOrDefault();

                    if (oneToMany != null
                        && oneToMany != Property)
                    {
                        return new Relation(Table.Type, oneToMany.PropertyType.GetMultipleRelationItemType(), oneToMany, _modelProvider);
                    }

                    if (Table.Type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                    {
                        var mtmTable = (MtmTableInfo)_modelProvider.Tables[Table.Type];

                        var type = Property.Name.Equals(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), StringComparison.OrdinalIgnoreCase)
                            ? mtmTable.Left
                            : mtmTable.Right;

                        return new Relation(Table.Type, type, Property, _modelProvider);
                    }

                    return default;
                }
            }
        }

        /// <summary>
        /// Is column relation
        /// </summary>
        /// <returns>Column is relation on not</returns>
        public bool IsRelation => Relation != null && !IsMultipleRelation;

        /// <summary>
        /// Is column multiple relation
        /// </summary>
        /// <returns>Column is multiple relation on not</returns>
        public bool IsMultipleRelation
        {
            get
            {
                _isMultipleRelation ??= new Lazy<bool>(InitIsMultipleRelation, LazyThreadSafetyMode.ExecutionAndPublication);
                return _isMultipleRelation.Value;

                bool InitIsMultipleRelation()
                {
                    return _chain.Any(property => property.PropertyType.IsMultipleRelation(out _));
                }
            }
        }

        /// <summary>
        /// Multiple relation table
        /// </summary>
        [NotNullIfNotNull(nameof(IsMultipleRelation))]
        public Type? MultipleRelationTable => IsMultipleRelation ? Property.ReflectedType : null;

        /// <summary>
        /// Is column inlined object
        /// </summary>
        /// <returns>Column is inlined object on not</returns>
        public bool IsInlinedObject
        {
            get
            {
                _isInlinedObject ??= new Lazy<bool>(InitIsInlinedObject, LazyThreadSafetyMode.ExecutionAndPublication);
                return _isInlinedObject.Value;

                bool InitIsInlinedObject()
                {
                    return _chain.Any(property => typeof(IInlinedObject).IsAssignableFrom(property.PropertyType));
                }
            }
        }

        /// <summary>
        /// Constraints
        /// </summary>
        public IReadOnlyCollection<string> Constraints
        {
            get
            {
                _constraints ??= InitConstraints()
                    .OrderBy(constraint => constraint)
                    .ToList();

                return _constraints;

                IEnumerable<string> InitConstraints()
                {
                    if (Name.Equals(nameof(IUniqueIdentified.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "primary key";
                    }
                    else if (Relation != null)
                    {
                        yield return $@"references ""{_modelProvider.SchemaName(Relation.Target)}"".""{_modelProvider.TableName(Relation.Target)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")";
                    }

                    if (!Property.Declared.IsNullable())
                    {
                        yield return "not null";
                    }
                }
            }
        }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ColumnInfo</param>
        /// <param name="right">Right ColumnInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(ColumnInfo? left, ColumnInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ColumnInfo</param>
        /// <param name="right">Right ColumnInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ColumnInfo? left, ColumnInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _chain.Aggregate(Table.GetHashCode(), HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ColumnInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ColumnInfo other)
        {
            return Table.Equals(other.Table)
                   && _chain.SequenceEqual(other._chain);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Table.Schema}.{Table.Name}.{Name} ({Constraints.ToString(", ")})";
        }

        /// <summary>
        /// Builds expression tree
        /// </summary>
        /// <param name="source">Source expression</param>
        /// <returns>Expression tree</returns>
        public Expression BuildExpression(Expression source)
        {
            if (source.Type != Table.Type)
            {
                throw new InvalidOperationException($"Expression should be constructed over {Table.Type.FullName} type instead of {source.Type.FullName}");
            }

            _expressionExtractor ??= GetValueExtractor<Expression>(_chain, MakeMemberAccess);

            return _expressionExtractor.Invoke(Expression.Parameter(Table.Type));

            static Expression MakeMemberAccess(PropertyInfo property, Expression expression)
            {
                return Expression.MakeMemberAccess(expression, property);
            }
        }

        /// <summary>
        /// Builds sql expression tree
        /// </summary>
        /// <param name="parameter">Parameter expression</param>
        /// <returns>Expression tree</returns>
        public IBindingSqlExpression BuildExpression(Translation.Expressions.ParameterExpression parameter)
        {
            if (parameter.Type != Table.Type)
            {
                throw new InvalidOperationException($"Parameter expression should be constructed over {Table.Type.FullName} type instead of {parameter.Type.FullName}");
            }

            _sqlExpressionExtractor ??= GetValueExtractor<ISqlExpression>(_chain, AggregateBindings);

            return (IBindingSqlExpression)_sqlExpressionExtractor.Invoke(parameter);

            static ISqlExpression AggregateBindings(PropertyInfo property, ISqlExpression source)
            {
                return new SimpleBindingExpression(property, property.PropertyType, source);
            }
        }

        /// <summary>
        /// Gets entity column value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Column value</returns>
        public object? GetValue(IUniqueIdentified entity)
        {
            if (IsMultipleRelation)
            {
                return null;
            }

            _valueExtractor ??= GetValueExtractor<object?>(_chain, AggregateValue);

            return _valueExtractor.Invoke(entity);
        }

        /// <summary>
        /// Gets entity relation value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Relation value</returns>
        public IUniqueIdentified? GetRelationValue(IUniqueIdentified entity)
        {
            if (IsRelation)
            {
                _relationValueExtractor ??= GetValueExtractor<object?>(_chain.SkipLast(1), AggregateValue);

                return _relationValueExtractor.Invoke(entity) as IUniqueIdentified;
            }

            return null;
        }

        /// <summary>
        /// Gets entity multiple relation value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Multiple relation value</returns>
        public IEnumerable<IUniqueIdentified> GetMultipleRelationValue(IUniqueIdentified entity)
        {
            if (IsMultipleRelation)
            {
                _relationValueExtractor ??= GetValueExtractor<object?>(_chain.SkipLast(1), AggregateValue);

                return ((IEnumerable)_relationValueExtractor.Invoke(entity) !).AsEnumerable<IUniqueIdentified>();
            }

            return Enumerable.Empty<IUniqueIdentified>();
        }

        /// <summary>
        /// Creates multiple relation entity (mtm)
        /// </summary>
        /// <param name="left">Left (owner)</param>
        /// <param name="right">Right (dependency)</param>
        /// <returns>IUniqueIdentified</returns>
        public IUniqueIdentified CreateMtm(
            IUniqueIdentified left,
            IUniqueIdentified right)
        {
            if (!IsMultipleRelation)
            {
                throw new InvalidOperationException($"Column {Name} should represent a multiple relation in order to create mtm entry");
            }

            _mtmCctor ??= GetMtmCctor(MultipleRelationTable!);

            return (IUniqueIdentified)_mtmCctor.Invoke(null, new[] { left.PrimaryKey, right.PrimaryKey });

            static MethodInfo GetMtmCctor(Type mtmTable)
            {
                var leftKey = mtmTable.ExtractGenericArgumentAt(typeof(BaseMtmDatabaseEntity<,>), 0);
                var rightKey = mtmTable.ExtractGenericArgumentAt(typeof(BaseMtmDatabaseEntity<,>), 1);

                return new MethodFinder(
                        typeof(ColumnInfo),
                        nameof(CreateMtmInstance),
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
                    {
                        TypeArguments = new[] { mtmTable, leftKey, rightKey },
                        ArgumentTypes = new[] { leftKey, rightKey }
                    }
                    .FindMethod()
                    .EnsureNotNull($"Could not find {nameof(CreateMtmInstance)} method")
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