namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// InstanceRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class InstanceRegistrationInfo : IRegistrationInfo,
                                            IEquatable<InstanceRegistrationInfo>,
                                            ISafelyEquatable<InstanceRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="instance">Instance</param>
        public InstanceRegistrationInfo(Type service, object instance)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            Instance = instance;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <summary>
        /// Instance
        /// </summary>
        public object Instance { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle => EnLifestyle.Singleton;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Instance.GetType(), Lifestyle);
        }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(InstanceRegistrationInfo other)
        {
            return Service == other.Service
                   && Instance == other.Instance;
        }

        /// <inheritdoc />
        public bool Equals(InstanceRegistrationInfo? other)
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
            return HashCode.Combine(Service, Instance);
        }

        #endregion
    }
}