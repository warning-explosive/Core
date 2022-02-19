namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// Column property
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ColumnProperty : IEquatable<ColumnProperty>,
                                  ISafelyEquatable<ColumnProperty>
    {
        /// <summary> .cctor </summary>
        /// <param name="declaredProperty">Declared property</param>
        /// <param name="reflectedProperty">Reflected property</param>
        public ColumnProperty(
            PropertyInfo declaredProperty,
            PropertyInfo reflectedProperty)
        {
            Declared = declaredProperty;
            Reflected = reflectedProperty;
        }

        /// <summary>
        /// Declared property
        /// </summary>
        public PropertyInfo Declared { get; }

        /// <summary>
        /// Reflected property
        /// </summary>
        public PropertyInfo Reflected { get; }

        /// <summary>
        /// Property name
        /// </summary>
        public string Name => Declared.Name;

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType => Declared.PropertyType;

        /// <summary>
        /// Declaring type
        /// </summary>
        public Type DeclaringType => Declared.DeclaringType ?? throw new InvalidOperationException("Property should have declaring type");

        /// <summary>
        /// Reflected type
        /// </summary>
        public Type ReflectedType => Reflected.ReflectedType ?? throw new InvalidOperationException("Property should have reflected type");

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ColumnProperty</param>
        /// <param name="right">Right ColumnProperty</param>
        /// <returns>equals</returns>
        public static bool operator ==(ColumnProperty? left, ColumnProperty? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ColumnProperty</param>
        /// <param name="right">Right ColumnProperty</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ColumnProperty? left, ColumnProperty? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Declared.GetHashCode(), ReflectedType.GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ColumnProperty? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ColumnProperty other)
        {
            return Declared.Equals(other.Declared)
                   && Reflected.Equals(other.Reflected);
        }

        #endregion
    }
}