namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Basics;

    /// <summary>
    /// TableInfo
    /// </summary>
    public class TableInfo : IObjectModelInfo
    {
        private IReadOnlyDictionary<string, IndexInfo>? _indexes;

        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="columns">Columns</param>
        public TableInfo(
            string schema,
            Type type,
            IReadOnlyCollection<ColumnInfo> columns)
        {
            Schema = schema;
            Type = type;
            Columns = columns.ToDictionary(info => info.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

        /// <summary>
        /// Indexes
        /// </summary>
        public IReadOnlyDictionary<string, IndexInfo> Indexes
        {
            get
            {
                _indexes ??= InitIndexes();

                return _indexes;

                IReadOnlyDictionary<string, IndexInfo> InitIndexes()
                {
                    return Type
                        .GetAttributes<IndexAttribute>()
                        .Select(index => new IndexInfo(Schema, Type, GetColumns(index).ToList(), index.Unique))
                        .ToDictionary(index => index.Name);

                    IEnumerable<ColumnInfo> GetColumns(IndexAttribute index)
                    {
                        foreach (var column in index.Columns)
                        {
                            if (!Columns.TryGetValue(column, out var info))
                            {
                                throw new InvalidOperationException($"Table {Schema}.{Type.Name} doesn't have column {column} for index");
                            }

                            yield return info;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Type.Name}";
        }
    }
}