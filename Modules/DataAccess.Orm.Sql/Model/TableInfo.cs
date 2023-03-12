namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Basics;

    internal class TableInfo : ITableInfo,
                               IEquatable<TableInfo>,
                               ISafelyEquatable<TableInfo>
    {
        private readonly IModelProvider _modelProvider;

        private IReadOnlyDictionary<string, IndexInfo>? _indexes;
        private IReadOnlyDictionary<string, ColumnInfo>? _columns;

        public TableInfo(
            Type type,
            IModelProvider modelProvider)
        {
            Type = type;

            _modelProvider = modelProvider;
        }

        public string Schema => _modelProvider.SchemaName(Type);

        public string Name => _modelProvider.TableName(Type);

        public Type Type { get; }

        public virtual bool IsMtmTable { get; } = false;

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
                                throw new InvalidOperationException($"Table {Schema}.{Name} doesn't have column {column} for index");
                            }

                            yield return info;
                        }
                    }
                }
            }
        }

        #region IEquatable

        public static bool operator ==(TableInfo? left, TableInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(TableInfo? left, TableInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(TableInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(TableInfo other)
        {
            return Type == other.Type;
        }

        #endregion

        public override string ToString()
        {
            return $"{Schema}.{Name}";
        }
    }
}