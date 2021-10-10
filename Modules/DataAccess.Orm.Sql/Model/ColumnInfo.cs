namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
                    return Chain.Select(property => property.Name).ToString("_");
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
        /// Type
        /// </summary>
        public IReadOnlyCollection<string> Constraints
        {
            get
            {
                _constraints ??= InitConstraints().ToList();
                return _constraints;

                IEnumerable<string> InitConstraints()
                {
                    if (Name.Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "primary key";
                    }

                    if (Type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
                    {
                        yield return $"references {Type.Name}";
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