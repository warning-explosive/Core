namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Api.Model;
    using Basics;
    using Orm.Model;

    /// <summary>
    /// ColumnInfo
    /// </summary>
    public class ColumnInfo : IModelInfo
    {
        private readonly PropertyInfo[] _chain;
        private readonly IModelProvider _modelProvider;

        private string? _name;
        private PropertyInfo? _property;
        private IReadOnlyCollection<string>? _constraints;
        private Lazy<Relation?>? _relation;
        private Lazy<bool>? _isMultipleRelation;

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
        /// <returns>Column is  multiple relation on not</returns>
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table.Name}.{Name} ({Constraints.ToString(", ")})";
        }
    }
}