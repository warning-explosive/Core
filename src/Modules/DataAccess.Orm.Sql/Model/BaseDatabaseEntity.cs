namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// Base class for database entities
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    public abstract record BaseDatabaseEntity<TKey> : IDatabaseEntity<TKey>,
                                                      ISafelyEquatable<BaseDatabaseEntity<TKey>>
        where TKey : notnull
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        protected BaseDatabaseEntity(TKey primaryKey)
        {
            PrimaryKey = primaryKey;
        }

        /// <inheritdoc />
        public TKey PrimaryKey { get; internal init; }

        object IUniqueIdentified.PrimaryKey => PrimaryKey;

        /// <inheritdoc />
        public long Version { get; set; }

        #region IEquatable

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(PrimaryKey, Version, GetType());
        }

        /// <inheritdoc />
        public virtual bool Equals(BaseDatabaseEntity<TKey>? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(BaseDatabaseEntity<TKey> other)
        {
            return PrimaryKey.Equals(other.PrimaryKey)
                && Version.Equals(other.Version)
                && GetType() == other.GetType();
        }

        #endregion
    }
}