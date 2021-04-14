namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using Basics;

    internal class DecoratorRegistrationInfo : ServiceRegistrationInfo,
                                               IEquatable<DecoratorRegistrationInfo>,
                                               ISafelyEquatable<DecoratorRegistrationInfo>
    {
        internal DecoratorRegistrationInfo(Type serviceType, Type implementationType)
            : base(serviceType, implementationType)
        {
        }

        internal Type? ConditionAttribute { get; set; }

        public bool SafeEquals(DecoratorRegistrationInfo other)
        {
            return base.SafeEquals(other)
                   && ConditionAttribute == other.ConditionAttribute;
        }

        public bool Equals(DecoratorRegistrationInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ConditionAttribute);
        }
    }
}