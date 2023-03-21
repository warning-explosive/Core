namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Basics;

    internal class IndexInfo : IModelInfo,
                               IEquatable<IndexInfo>,
                               ISafelyEquatable<IndexInfo>
    {
        public IndexInfo(
            ITableInfo table,
            IReadOnlyCollection<ColumnInfo> columns,
            IReadOnlyCollection<ColumnInfo> includedColumns,
            bool unique,
            string? predicate)
        {
            Table = table;
            Columns = columns;
            IncludedColumns = includedColumns;
            Unique = unique;
            Predicate = predicate;
        }

        public ITableInfo Table { get; }

        public IReadOnlyCollection<ColumnInfo> Columns { get; }

        public IReadOnlyCollection<ColumnInfo> IncludedColumns { get; }

        public string Name => string.Join(
            "__",
            Table.Name,
            string.Join("_", Columns.OrderBy(column => column.Name).Select(column => column.Name)));

        public bool Unique { get; }

        public string? Predicate { get; }

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
            var sb = new StringBuilder();

            sb.Append($"{Table.Schema}.{Name}");

            var attributes = new List<string>();

            if (Unique)
            {
                attributes.Add("Unique");
            }

            if (!Predicate.IsNullOrWhiteSpace())
            {
                attributes.Add(Predicate);
            }

            if (attributes.Any())
            {
                sb.Append(" ");
                sb.Append(attributes.ToString(", "));
            }

            return sb.ToString();
        }
    }
}