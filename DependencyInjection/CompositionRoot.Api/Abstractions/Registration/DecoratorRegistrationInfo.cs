namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DecoratorRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class DecoratorRegistrationInfo : IEquatable<DecoratorRegistrationInfo>,
                                             ISafelyEquatable<DecoratorRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public DecoratorRegistrationInfo(Type service, Type implementation, EnLifestyle lifestyle)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            Implementation = implementation;
            Lifestyle = lifestyle;
        }

        /// <summary>
        /// Service
        /// </summary>
        public Type Service { get; }

        /// <summary>
        /// Implementation
        /// </summary>
        public Type Implementation { get; }

        /// <summary>
        /// Lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Implementation, Lifestyle);
        }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(DecoratorRegistrationInfo other)
        {
            return Service == other.Service
                   && Implementation == other.Implementation
                   && Lifestyle == other.Lifestyle;
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
            return HashCode.Combine(Service, Implementation, Lifestyle);
        }

        #endregion
    }
}