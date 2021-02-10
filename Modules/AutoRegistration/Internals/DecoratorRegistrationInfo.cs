namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using AutoWiringApi.Enumerations;

    internal class DecoratorRegistrationInfo : ServiceRegistrationInfo, IEquatable<DecoratorRegistrationInfo>
    {
        internal DecoratorRegistrationInfo(Type serviceType, Type implementationType, EnLifestyle lifestyle)
            : base(serviceType, implementationType, lifestyle)
        {
        }

        internal Type? Attribute { get; set; }

        public bool Equals(DecoratorRegistrationInfo? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                   && Attribute == other.Attribute;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((DecoratorRegistrationInfo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Attribute);
        }
    }
}