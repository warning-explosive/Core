namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DelegateRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class DelegateRegistrationInfo : IEquatable<DelegateRegistrationInfo>,
                                            ISafelyEquatable<DelegateRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public DelegateRegistrationInfo(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            InstanceProducer = instanceProducer;
            Lifestyle = lifestyle;
        }

        /// <summary>
        /// Service
        /// </summary>
        public Type Service { get; }

        /// <summary>
        /// Instance producer
        /// </summary>
        public Func<object> InstanceProducer { get; }

        /// <summary>
        /// Lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Lifestyle);
        }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(DelegateRegistrationInfo other)
        {
            return Service == other.Service
                   && InstanceProducer == other.InstanceProducer
                   && Lifestyle == other.Lifestyle;
        }

        /// <inheritdoc />
        public bool Equals(DelegateRegistrationInfo? other)
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
            return HashCode.Combine(Service, InstanceProducer, Lifestyle);
        }

        #endregion
    }
}