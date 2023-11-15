namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Basics;

    internal class ViewInfo : ITableInfo,
                              IEquatable<ViewInfo>,
                              ISafelyEquatable<ViewInfo>
    {
        private readonly IModelProvider _modelProvider;

        private IReadOnlyDictionary<string, IndexInfo>? _indexes;

        public ViewInfo(
            Type type,
            string query,
            IReadOnlyDictionary<string, ColumnInfo> columns,
            IModelProvider modelProvider)
        {
            Type = type;
            Query = query;
            Columns = columns;

            _modelProvider = modelProvider;
        }

        public string Schema => _modelProvider.SchemaName(Type);

        public string Name => _modelProvider.TableName(Type);

        public Type Type { get; }

        public bool IsMtmTable => false;

        public IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

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
                        .Select(index => new IndexInfo(
                            this,
                            GetColumns(index.Columns).ToList(),
                            GetColumns(index.IncludedColumns).ToList(),
                            index.Unique,
                            index.Predicate))
                        .ToDictionary(index => index.Name);

                    IEnumerable<ColumnInfo> GetColumns(IEnumerable<string> indexColumns)
                    {
                        foreach (var column in indexColumns)
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

        public override string ToString()
        {
            return $"{Schema}.{Name} ({Query})";
        }
    }
}