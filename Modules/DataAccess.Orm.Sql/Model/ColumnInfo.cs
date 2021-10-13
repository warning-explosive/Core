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
        private string? _name;
        private PropertyInfo? _property;
        private IReadOnlyCollection<string>? _constraints;
        private Lazy<Relation?>? _relation;
        private Lazy<Relation?>? _multipleRelation;

        /// <summary> .cctor </summary>
        /// <param name="tableType">Table type</param>
        /// <param name="chain">Property chain</param>
        public ColumnInfo(Type tableType, IReadOnlyCollection<PropertyInfo> chain)
        {
            TableType = tableType;
            Chain = chain;
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
        /// Property chain
        /// </summary>
        public IReadOnlyCollection<PropertyInfo> Chain { get; }

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
        /// Type
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
                    var property = Chain
                        .Reverse()
                        .FirstOrDefault(property => property.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)));

                    return property == null
                        ? null
                        : new Relation(property, property.PropertyType);
                }
            }
        }

        /// <summary>
        /// Multiple relation
        /// </summary>
        public Relation? MultipleRelation
        {
            get
            {
                _multipleRelation ??= new Lazy<Relation?>(InitMultipleRelation, LazyThreadSafetyMode.ExecutionAndPublication);
                return _multipleRelation.Value;

                Relation? InitMultipleRelation()
                {
                    foreach (var property in Chain.Reverse())
                    {
                        if (property.PropertyType.IsMultipleRelation(out var itemType))
                        {
                            return new Relation(property, itemType);
                        }
                    }

                    return default;
                }
            }
        }

        /// <summary>
        /// Type
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
                        yield return $@"references ""{Relation.Type.SchemaName()}"".""{Relation.Type.Name}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")";
                    }
                    else if (TableType.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<>)))
                    {
                        var parts = TableType.Name.Split('_', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length != 5)
                        {
                            throw new InvalidOperationException($"MtM table name should contain 5 parts: {TableType.Name}");
                        }

                        if (Name.Equals(nameof(BaseMtmDatabaseEntity<Guid>.Left), StringComparison.OrdinalIgnoreCase))
                        {
                            var schema = parts[0];
                            var table = parts[1];

                            yield return $@"references ""{schema}"".""{table}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")";
                        }
                        else if (Name.Equals(nameof(BaseMtmDatabaseEntity<Guid>.Right), StringComparison.OrdinalIgnoreCase))
                        {
                            var schema = parts[3];
                            var table = parts[4];

                            yield return $@"references ""{schema}"".""{table}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")";
                        }
                    }

                    if (!Property.IsNullable())
                    {
                        yield return "not null";
                    }
                }
            }
        }
    }
}