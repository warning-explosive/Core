namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// ComponentOverrideInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ComponentOverrideInfo : IEquatable<ComponentOverrideInfo>,
                                         ISafelyEquatable<ComponentOverrideInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="replacement">Implementation replacement</param>
        /// <param name="lifestyle">Implementation replacement lifestyle</param>
        public ComponentOverrideInfo(
            Type service,
            Type implementation,
            Type replacement,
            EnLifestyle lifestyle)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            Implementation = implementation;
            Replacement = replacement;
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
        /// Implementation replacement
        /// </summary>
        public Type Replacement { get; }

        /// <summary>
        /// Implementation replacement lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Implementation, Replacement, Lifestyle);
        }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(ComponentOverrideInfo other)
        {
            return Service == other.Service
                   && Implementation == other.Implementation
                   && Replacement == other.Replacement
                   && Lifestyle == other.Lifestyle;
        }

        /// <inheritdoc />
        public bool Equals(ComponentOverrideInfo? other)
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
            return HashCode.Combine(Service, Implementation, Replacement, Lifestyle);
        }

        #endregion
    }
}