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
    using Orm.Model;
    using Translation.Expressions;

    /// <summary>
    /// ColumnInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ColumnInfo : IModelInfo,
                              IEquatable<ColumnInfo>,
                              ISafelyEquatable<ColumnInfo>
    {
        private readonly PropertyInfo[] _chain;
        private readonly IModelProvider _modelProvider;

        private string? _name;
        private PropertyInfo? _property;
        private IReadOnlyCollection<string>? _constraints;
        private Lazy<Relation?>? _relation;
        private Lazy<bool>? _isMultipleRelation;
        private Lazy<bool>? _isInlinedObject;

        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="chain">Property chain</param>
        /// <param name="modelProvider">IModelProvider</param>
        public ColumnInfo(
            string schema,
            Type table,
            PropertyInfo[] chain,
            IModelProvider modelProvider)
        {
            Schema = schema;
            Table = table;

            if (!chain.Any())
            {
                throw new InvalidOperationException("Column chain should contain at least one property info");
            }

            _chain = chain;

            _modelProvider = modelProvider;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table type
        /// </summary>
        public Type Table { get; }

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
        /// Type
        /// </summary>
        public Type Type => Property.PropertyType;

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
                        return new Relation(Table, oneToOne.PropertyType, oneToOne);
                    }

                    var oneToMany = _chain
                        .Reverse()
                        .SkipWhile(property => property.ReflectedType.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                        .FirstOrDefault();

                    if (oneToMany != Property)
                    {
                        if (oneToMany != null)
                        {
                            return new Relation(Table, oneToMany.PropertyType.GetMultipleRelationItemType(), oneToMany);
                        }
                    }

                    var manyToMany = Property.ReflectedType;

                    if (manyToMany.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                    {
                        var (left, right) = _modelProvider.MtmTables[Schema][manyToMany];

                        var type = Property.Name.Equals(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), StringComparison.OrdinalIgnoreCase)
                            ? left
                            : right;

                        return new Relation(Table, type, Property);
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
                    if (Name.Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "primary key";
                    }
                    else if (Relation != null)
                    {
                        yield return $@"references ""{Relation.Target.SchemaName()}"".""{Relation.Target.TableName()}"" (""{nameof(IUniqueIdentified<Guid>.PrimaryKey)}"")";
                    }

                    if (!Property.IsNullable())
                    {
                        yield return "not null";
                    }
                }
            }
        }

        private PropertyInfo Property
        {
            get
            {
                _property ??= InitProperty();
                return _property;

                PropertyInfo InitProperty()
                {
                    return _chain.Last();
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
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return new object[] { Table }
                .Concat(_chain.Select(property => property))
                .Aggregate(Schema.GetHashCode(StringComparison.OrdinalIgnoreCase), HashCode.Combine);
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
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Table == other.Table
                   && _chain.SequenceEqual(other._chain);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table.TableName()}.{Name} ({Constraints.ToString(", ")})";
        }

        /// <summary>
        /// Builds expression tree
        /// </summary>
        /// <param name="source">Source expression</param>
        /// <returns>Expression tree</returns>
        public Expression BuildExpression(Expression source)
        {
            if (source.Type != Table)
            {
                throw new InvalidOperationException($"Expression should be constructed over {Table.FullName} type instead of {source.Type.FullName}");
            }

            return _chain.Aggregate(
                (Expression)Expression.Parameter(Table),
                Expression.MakeMemberAccess);
        }

        /// <summary>
        /// Builds intermediate expression tree
        /// </summary>
        /// <param name="parameter">Parameter expression</param>
        /// <returns>Expression tree</returns>
        public IBindingIntermediateExpression BuildExpression(Translation.Expressions.ParameterExpression parameter)
        {
            if (parameter.Type != Table)
            {
                throw new InvalidOperationException($"Parameter expression should be constructed over {Table.FullName} type instead of {parameter.Type.FullName}");
            }

            var chain = _chain.Aggregate(
                (IIntermediateExpression)parameter,
                (acc, next) => new SimpleBindingExpression(next, next.PropertyType, acc));

            return (IBindingIntermediateExpression)chain;
        }

        /// <summary>
        /// Gets entity column value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Column value</returns>
        public object? GetValue<TKey>(IUniqueIdentified<TKey> entity)
            where TKey : notnull
        {
            return IsMultipleRelation
                ? null
                : _chain.Aggregate((object?)entity, AggregateValue);
        }

        /// <summary>
        /// Gets entity relation value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Relation value</returns>
        public IUniqueIdentified<TKey>? GetRelationValue<TKey>(IUniqueIdentified<TKey> entity)
            where TKey : notnull
        {
            if (!IsRelation)
            {
                return null;
            }

            return _chain
                    .SkipLast(1)
                    .Aggregate((object?)entity, AggregateValue) as IUniqueIdentified<TKey>;
        }

        /// <summary>
        /// Gets entity multiple relation value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Multiple relation value</returns>
        public IEnumerable<IUniqueIdentified<TKey>> GetMultipleRelationValue<TKey>(IUniqueIdentified<TKey> entity)
            where TKey : notnull
        {
            if (!IsMultipleRelation)
            {
                return Enumerable.Empty<IUniqueIdentified<TKey>>();
            }

            return ((IEnumerable)_chain
                    .SkipLast(1)
                    .Aggregate((object?)entity, AggregateValue) !)
                .AsEnumerable<IUniqueIdentified<TKey>>();
        }

        private static object? AggregateValue(object? value, PropertyInfo property)
        {
            if (value == null)
            {
                return null;
            }

            return property.GetValue(value);
        }
    }
}