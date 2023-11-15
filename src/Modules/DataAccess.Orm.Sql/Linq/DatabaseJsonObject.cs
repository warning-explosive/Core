namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// DatabaseJsonObject
    /// </summary>
    public class DatabaseJsonObject : IEquatable<DatabaseJsonObject>,
                                      ISafelyEquatable<DatabaseJsonObject>
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public DatabaseJsonObject(object? value, Type type)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Value
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left DatabaseJsonObject</param>
        /// <param name="right">Right DatabaseJsonObject</param>
        /// <returns>equals</returns>
        public static bool operator ==(DatabaseJsonObject? left, DatabaseJsonObject? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left DatabaseJsonObject</param>
        /// <param name="right">Right DatabaseJsonObject</param>
        /// <returns>not equals</returns>
        public static bool operator !=(DatabaseJsonObject? left, DatabaseJsonObject? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(DatabaseJsonObject other)
        {
            return Type == other.Type
                   && Value.Equals(other.Value);
        }

        /// <inheritdoc />
        public bool Equals(DatabaseJsonObject? other)
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
            return HashCode.Combine(Type, Value);
        }

        #endregion
    }

    /// <summary>
    /// DatabaseJsonObject
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1402", Justification = "DatabaseJsonObject")]
    public class DatabaseJsonObject<T> : DatabaseJsonObject
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        public DatabaseJsonObject(T value)
            : base(value, typeof(T))
        {
        }

        /// <summary>
        /// Typed value
        /// </summary>
        public T TypedValue => (T)Value!;
    }
}