namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using Api.Persisting;
    using Basics;
    using Sql.Model;

    internal class InsertSqlExpressionCacheKey : IEquatable<InsertSqlExpressionCacheKey>,
                                                 ISafelyEquatable<InsertSqlExpressionCacheKey>
    {
        public InsertSqlExpressionCacheKey(ITableInfo table, EnInsertBehavior insertBehavior)
        {
            Table = table;
            InsertBehavior = insertBehavior;
        }

        public ITableInfo Table { get; }

        public EnInsertBehavior InsertBehavior { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left InsertSqlExpressionCacheKey</param>
        /// <param name="right">Right InsertSqlExpressionCacheKey</param>
        /// <returns>equals</returns>
        public static bool operator ==(InsertSqlExpressionCacheKey? left, InsertSqlExpressionCacheKey? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left InsertSqlExpressionCacheKey</param>
        /// <param name="right">Right InsertSqlExpressionCacheKey</param>
        /// <returns>not equals</returns>
        public static bool operator !=(InsertSqlExpressionCacheKey? left, InsertSqlExpressionCacheKey? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(InsertSqlExpressionCacheKey other)
        {
            return Table.Equals(other.Table)
                   && InsertBehavior == other.InsertBehavior;
        }

        /// <inheritdoc />
        public bool Equals(InsertSqlExpressionCacheKey? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Table.GetHashCode(), (int)InsertBehavior);
        }

        #endregion

        public void Deconstruct(
            out ITableInfo table,
            out EnInsertBehavior insertBehavior)
        {
            table = Table;
            insertBehavior = InsertBehavior;
        }
    }
}