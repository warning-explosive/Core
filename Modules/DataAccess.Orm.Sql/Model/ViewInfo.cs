namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Sql.Attributes;
    using Basics;

    /// <summary>
    /// ViewInfo
    /// </summary>
    public class ViewInfo : ITableInfo,
                            IEquatable<ViewInfo>,
                            ISafelyEquatable<ViewInfo>
    {
        private readonly IModelProvider _modelProvider;

        private IReadOnlyDictionary<string, IndexInfo>? _indexes;
        private IReadOnlyDictionary<string, ColumnInfo>? _columns;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="query">Query</param>
        /// <param name="modelProvider">IModelProvider</param>
        public ViewInfo(
            Type type,
            string query,
            IModelProvider modelProvider)
        {
            Type = type;
            Query = query;

            _modelProvider = modelProvider;
        }

        /// <inheritdoc />
        public string Schema => _modelProvider.SchemaName(Type);

        /// <inheritdoc />
        public string Name => _modelProvider.TableName(Type);

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyDictionary<string, ColumnInfo> Columns
        {
            get
            {
                _columns ??= InitColumns();

                return _columns;

                IReadOnlyDictionary<string, ColumnInfo> InitColumns()
                {
                    return _modelProvider.Columns(this)
                        .OrderBy(column => column.Name)
                        .ToDictionary(info => info.Name, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

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
                        .Select(index => new IndexInfo(this, GetColumns(index).ToList(), index.Unique))
                        .ToDictionary(index => index.Name);

                    IEnumerable<ColumnInfo> GetColumns(IndexAttribute index)
                    {
                        foreach (var column in index.Columns)
                        {
                            if (!Columns.TryGetValue(column, out var info))
                            {
                                throw new InvalidOperationException($"View {Schema}.{Name} doesn't have column {column} for index");
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
        public override int GetHashCode()
        {
            return Type.GetHashCode();
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
            return Type == other.Type;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Name} ({Query})";
        }
    }
}