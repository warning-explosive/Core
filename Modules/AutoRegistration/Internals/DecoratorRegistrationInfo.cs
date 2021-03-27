namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;

    internal class DecoratorRegistrationInfo : ServiceRegistrationInfo, IEquatable<DecoratorRegistrationInfo>
    {
        internal DecoratorRegistrationInfo(Type serviceType, Type implementationType)
            : base(serviceType, implementationType)
        {
        }

        internal Type? ConditionAttribute { get; set; }

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

            return base.Equals(other) && ConditionAttribute == other.ConditionAttribute;
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
            return HashCode.Combine(base.GetHashCode(), ConditionAttribute);
        }
    }
}