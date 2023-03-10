namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    internal class IndexInfo : IModelInfo,
                               IEquatable<IndexInfo>,
                               ISafelyEquatable<IndexInfo>
    {
        public IndexInfo(
            ITableInfo table,
            IReadOnlyCollection<ColumnInfo> columns,
            bool unique)
        {
            Table = table;
            Columns = columns;
            Unique = unique;
        }

        public ITableInfo Table { get; }

        public IReadOnlyCollection<ColumnInfo> Columns { get; }

        public string Name => string.Join(
            "__",
            Table.Name,
            string.Join("_", Columns.OrderBy(column => column.Name).Select(column => column.Name)));

        public bool Unique { get; }

        #region IEquatable

        public static bool operator ==(IndexInfo? left, IndexInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(IndexInfo? left, IndexInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(IndexInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(IndexInfo other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public override string ToString()
        {
            return $"{Table.Schema}.{Table.Name}.{Name} ({Unique})";
        }
    }
}