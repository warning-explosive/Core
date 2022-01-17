namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Model;
    using Basics;

    /// <summary>
    /// ViewInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ViewInfo : IObjectModelInfo,
                            IEquatable<ViewInfo>,
                            ISafelyEquatable<ViewInfo>
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
            Columns = columns
                .OrderBy(column => column.Name)
                .ToDictionary(info => info.Name, StringComparer.OrdinalIgnoreCase);
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

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ViewInfo</param>
        /// <param name="right">Right ViewInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(ViewInfo? left, ViewInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ViewInfo</param>
        /// <param name="right">Right ViewInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ViewInfo? left, ViewInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return new object[] { Type }
                .Concat(Columns.Values.OrderBy(column => column.Name))
                .Aggregate(Schema.GetHashCode(StringComparison.OrdinalIgnoreCase), HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ViewInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ViewInfo other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Type == other.Type
                   && Columns.Values.OrderBy(column => column.Name).SequenceEqual(other.Columns.Values.OrderBy(column => column.Name));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Type.Name} ({Query})";
        }
    }
}