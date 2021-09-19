namespace SpaceEngineers.Core.DataAccess.Api.DatabaseEntity
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// Base class for database entities
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public abstract class BaseDatabaseEntity<TKey> : IDatabaseEntity<TKey>,
                                                     IEquatable<BaseDatabaseEntity<TKey>>,
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
        public TKey PrimaryKey { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left BaseDatabaseEntity</param>
        /// <param name="right">Right BaseDatabaseEntity</param>
        /// <returns>equals</returns>
        public static bool operator ==(BaseDatabaseEntity<TKey>? left, BaseDatabaseEntity<TKey>? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left BaseDatabaseEntity</param>
        /// <param name="right">Right BaseDatabaseEntity</param>
        /// <returns>not equals</returns>
        public static bool operator !=(BaseDatabaseEntity<TKey>? left, BaseDatabaseEntity<TKey>? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(PrimaryKey, GetType());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(BaseDatabaseEntity<TKey>? other)
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