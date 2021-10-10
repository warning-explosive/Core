namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Basics;
    using Orm.Model;

    /// <summary>
    /// TableInfo
    /// </summary>
    public class TableInfo : IObjectModelInfo
    {
        private IReadOnlyDictionary<string, IndexInfo>? _indexes;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="columns">Columns</param>
        public TableInfo(Type type, IReadOnlyCollection<ColumnInfo> columns)
        {
            Type = type;
            Columns = columns.ToDictionary(info => info.Name);
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

                            throw new InvalidOperationException($"Table {Schema}.{Name} doesn't have column {column} for index");
                        }
                    }
                }
            }
        }
    }
}