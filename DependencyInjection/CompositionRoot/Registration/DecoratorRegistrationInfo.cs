namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DecoratorRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class DecoratorRegistrationInfo : ServiceRegistrationInfo,
                                             IEquatable<DecoratorRegistrationInfo>,
                                             ISafelyEquatable<DecoratorRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public DecoratorRegistrationInfo(Type service, Type implementation, EnLifestyle lifestyle)
            : base(service, implementation, lifestyle)
        {
        }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(DecoratorRegistrationInfo other)
        {
            return base.SafeEquals(other);
        }

        /// <inheritdoc />
        public bool Equals(DecoratorRegistrationInfo? other)
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
            return base.GetHashCode();
        }

        #endregion
    }
}