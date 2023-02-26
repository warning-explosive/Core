namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Sql.Attributes;
    using Basics;

    /// <summary>
    /// TableInfo
    /// </summary>
    public class TableInfo : ITableInfo,
                             IEquatable<TableInfo>,
                             ISafelyEquatable<TableInfo>
    {
        private readonly IModelProvider _modelProvider;

        private IReadOnlyDictionary<string, IndexInfo>? _indexes;
        private IReadOnlyDictionary<string, ColumnInfo>? _columns;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="modelProvider">IModelProvider</param>
        public TableInfo(
            Type type,
            IModelProvider modelProvider)
        {
            Type = type;

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

        /// <inheritdoc />
        public virtual bool IsMtmTable { get; } = false;

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
                    return _modelProvider
                        .Columns(this)
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
                                throw new InvalidOperationException($"Table {Schema}.{Name} doesn't have column {column} for index");
                            }

                            yield return info;
                        }
                    }
                }
            }
        }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left TableInfo</param>
        /// <param name="right">Right TableInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(TableInfo? left, TableInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left TableInfo</param>
        /// <param name="right">Right TableInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(TableInfo? left, TableInfo? right)
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
        public bool Equals(TableInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(TableInfo other)
        {
            return Type == other.Type;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Name}";
        }
    }
}