namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Api.Model;
    using Basics;
    using Orm.Model;

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
        /// Property
        /// </summary>
        public PropertyInfo Property
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
                        return new Relation(oneToOne.PropertyType, oneToOne.Name);
                    }

                    var oneToMany = _chain
                        .Reverse()
                        .SkipWhile(property => property.ReflectedType.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                        .FirstOrDefault();

                    if (oneToMany != Property)
                    {
                        if (oneToMany != null)
                        {
                            return new Relation(oneToMany.PropertyType, oneToMany.Name);
                        }
                    }

                    var manyToMany = Property.ReflectedType;

                    if (manyToMany.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                    {
                        var (left, right) = _modelProvider.MtmTables[Schema][manyToMany];

                        var type = Property.Name.Equals(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), StringComparison.OrdinalIgnoreCase)
                            ? left
                            : right;

                        return new Relation(type, nameof(IUniqueIdentified<Guid>.PrimaryKey));
                    }

                    return default;
                }
            }
        }

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
                        yield return $@"references ""{Relation.Type.SchemaName()}"".""{Relation.Type.Name}"" (""{nameof(IUniqueIdentified<Guid>.PrimaryKey)}"")";
                    }

                    if (!Property.IsNullable())
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
            return $"{Schema}.{Table.Name}.{Name} ({Constraints.ToString(", ")})";
        }

        /// <summary>
        /// Gets entity column value
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Column value</returns>
        public object? GetValue<TEntity, TKey>(TEntity entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            object? value = entity;

            foreach (var property in _chain)
            {
                if (value == null)
                {
                    break;
                }

                value = property.GetValue(value);
            }

            return value;
        }
    }
}