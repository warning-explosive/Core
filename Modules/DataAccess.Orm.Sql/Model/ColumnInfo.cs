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
                    else if (Property.Name.Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                    {
                        var relation = Chain.SkipLast(1).Last();

                        if (relation.PropertyType.IsMultipleRelation(out var itemType))
                        {
                            // TODO: #110 - references to intermediate many-to-many table
                            yield return @"references ""schema"".""to_do_many_to_many_table""";
                        }
                        else
                        {
                            yield return $@"references ""{relation.PropertyType.SchemaName()}"".""{relation.PropertyType.Name}""";
                        }
                    }

                    if (!Property.IsNullable())
                    {
                        yield return "not null";
                    }
                }
            }
        }

        /// <summary>
        /// Gets DB constraints
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        /// <param name="nullable">Nullable</param>
        /// <param name="modelProvider">IModelProvider</param>
        /// <returns>Constraints</returns>
        public static IEnumerable<string> DbConstraints(
            string schema,
            string table,
            string column,
            bool nullable,
            IModelProvider modelProvider)
        {
            if (column.Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
            {
                yield return "primary key";
            }
            else if (column.Split("_", StringSplitOptions.RemoveEmptyEntries).Last().Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
            {
                if (!modelProvider.Model.TryGetValue(schema, out var schemaInfo)
                    || !schemaInfo.TryGetValue(table, out var tableInfo)
                    || !tableInfo.Columns.TryGetValue(column, out var columnInfo))
                {
                    throw new InvalidOperationException($"{schema}.{table}.{column} isn't presented in the model");
                }

                if (columnInfo.Type.IsMultipleRelation(out var itemType))
                {
                    // TODO: #110 - references to intermediate many-to-many table
                    yield return $@"references ""schema"".""to_do_many_to_many_table""";
                }
                else
                {
                    yield return $@"references ""{columnInfo.Type.SchemaName()}"".""{columnInfo.Type.Name}""";
                }
            }

            if (!nullable)
            {
                yield return "not null";
            }
        }
    }
}