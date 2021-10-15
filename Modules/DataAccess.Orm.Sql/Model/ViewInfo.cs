namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Basics;

    /// <summary>
    /// ViewInfo
    /// </summary>
    public class ViewInfo : IObjectModelInfo
    {
        private IReadOnlyDictionary<string, IndexInfo>? _indexes;

        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="columns">Columns</param>
        /// <param name="query">Query</param>
        public ViewInfo(
            string schema,
            Type type,
            IReadOnlyCollection<ColumnInfo> columns,
            string query)
        {
            Schema = schema;
            Type = type;
            Columns = columns.ToDictionary(info => info.Name, StringComparer.OrdinalIgnoreCase);
            Query = query;
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
                                throw new InvalidOperationException($"View {Schema}.{Type.Name} doesn't have column {column} for index");
                            }

                            yield return info;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Query
        /// </summary>
        public bool Materialized => Indexes.Any();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Type.Name} ({Materialized}, {Query})";
        }
    }
}