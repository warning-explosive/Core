namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Reflection;
    using Basics;

    internal class ColumnProperty : IEquatable<ColumnProperty>,
                                    ISafelyEquatable<ColumnProperty>
    {
        public ColumnProperty(
            PropertyInfo declaredProperty,
            PropertyInfo reflectedProperty)
        {
            Declared = declaredProperty;
            Reflected = reflectedProperty;
        }

        public PropertyInfo Declared { get; }

        public PropertyInfo Reflected { get; }

        public string Name => Declared.Name;

        public Type PropertyType => Declared.PropertyType;

        public Type DeclaringType => Declared.DeclaringType ?? throw new InvalidOperationException("Property should have declaring type");

        public Type ReflectedType => Reflected.ReflectedType ?? throw new InvalidOperationException("Property should have reflected type");

        #region IEquatable

        public static bool operator ==(ColumnProperty? left, ColumnProperty? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(ColumnProperty? left, ColumnProperty? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Declared.GetHashCode(), ReflectedType.GetHashCode());
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(ColumnProperty? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(ColumnProperty other)
        {
            return Declared.Equals(other.Declared)
                   && Reflected.Equals(other.Reflected);
        }

        #endregion
    }
}