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
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> _tableTypes;

        private string? _name;
        private PropertyInfo? _property;
        private IReadOnlyCollection<string>? _constraints;
        private Lazy<Relation?>? _relation;
        private Lazy<bool>? _isMultipleRelation;

        /// <summary> .cctor </summary>
        /// <param name="tableType">Table type</param>
        /// <param name="chain">Property chain</param>
        /// <param name="tableTypes">Table types</param>
        public ColumnInfo(
            Type tableType,
            IReadOnlyCollection<PropertyInfo> chain,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> tableTypes)
        {
            TableType = tableType;
            Chain = chain;

            _tableTypes = tableTypes;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema => TableType.SchemaName();

        /// <summary>
        /// Table
        /// </summary>
        public string Table => TableType.Name;

        /// <summary>
        /// Table type
        /// </summary>
        public Type TableType { get; }

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
                    return Chain
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
                    return Chain.Last();
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
                    var oneToOne = Chain
                        .Reverse()
                        .FirstOrDefault(property => property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)));

                    if (oneToOne != null)
                    {
                        return new Relation(oneToOne.PropertyType, oneToOne.Name);
                    }

                    var oneToMany = Chain
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
                        var parts = TableType
                            .Name
                            .Split('_', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length != 5)
                        {
                            throw new InvalidOperationException($"MtM table name should contain 5 parts: {TableType.Name}");
                        }

                        string schema;
                        string table;

                        if (Property.Name.Equals(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), StringComparison.OrdinalIgnoreCase))
                        {
                            schema = parts[0];
                            table = parts[1];
                        }
                        else
                        {
                            schema = parts[3];
                            table = parts[4];
                        }

                        return new Relation(_tableTypes[schema][table], nameof(IUniqueIdentified<Guid>.PrimaryKey));
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
                    return Chain.Any(property => property.PropertyType.IsMultipleRelation(out _));
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

        /// <summary>
        /// Property chain
        /// </summary>
        private IReadOnlyCollection<PropertyInfo> Chain { get; }
    }
}