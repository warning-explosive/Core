namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// Base class for database entities
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    public abstract record BaseDatabaseEntity<TKey> : IDatabaseEntity<TKey>,
                                                      ISafelyEquatable<BaseDatabaseEntity<TKey>>
    {
        /// <summary>
        /// .cctor
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        protected BaseDatabaseEntity(TKey primaryKey)
        {
            // TODO: #149 - generate primary key in database
            PrimaryKey = primaryKey;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public TKey PrimaryKey { get; private init; }

        #region IEquatable

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(PrimaryKey, GetType());
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
                   && GetType() == other.GetType();
        }

        #endregion
    }
}