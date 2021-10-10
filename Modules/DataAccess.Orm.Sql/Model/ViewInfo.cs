namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Basics;
    using Orm.Model;

    /// <summary>
    /// ViewInfo
    /// </summary>
    public class ViewInfo : IObjectModelInfo
    {
        private IReadOnlyDictionary<string, IndexInfo>? _indexes;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="columns">Columns</param>
        /// <param name="query">Query</param>
        public ViewInfo(
            Type type,
            IReadOnlyCollection<ColumnInfo> columns,
            string query)
        {
            Type = type;
            Columns = columns.ToDictionary(info => info.Name);
            Query = query;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema => Type.SchemaName();

        /// <summary>
        /// Name
        /// </summary>
        public string Name => Type.Name;

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
                        .Select(index => new IndexInfo(Type, GetColumns(index).ToList(), index.Unique))
                        .ToDictionary(index => index.ToString());

                    IEnumerable<ColumnInfo> GetColumns(IndexAttribute index)
                    {
                        foreach (var column in index.Columns)
                        {
                            if (Columns.TryGetValue(column, out var info))
                            {
                                yield return info;
                            }

                            throw new InvalidOperationException($"View {Schema}.{Name} doesn't have column {column} for index");
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
    }
}