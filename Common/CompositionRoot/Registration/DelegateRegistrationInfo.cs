namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DelegateRegistrationInfo
    /// </summary>
    public class DelegateRegistrationInfo : IRegistrationInfo,
                                            IEquatable<DelegateRegistrationInfo>,
                                            ISafelyEquatable<DelegateRegistrationInfo>
    {
        private readonly Func<object> _instanceProducer;

        private object? _instance;

        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public DelegateRegistrationInfo(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            if (lifestyle != EnLifestyle.Singleton)
            {
                throw new NotSupportedException($"Delegates support only {EnLifestyle.Singleton} lifestyle due to the fact that they are always capture static state at application's startup");
            }

            Service = service.GenericTypeDefinitionOrSelf();
            Lifestyle = lifestyle;

            _instanceProducer = instanceProducer;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left DelegateRegistrationInfo</param>
        /// <param name="right">Right DelegateRegistrationInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(DelegateRegistrationInfo? left, DelegateRegistrationInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left DelegateRegistrationInfo</param>
        /// <param name="right">Right DelegateRegistrationInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(DelegateRegistrationInfo? left, DelegateRegistrationInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(DelegateRegistrationInfo other)
        {
            return Service == other.Service
                   && _instanceProducer == other._instanceProducer
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
            return HashCode.Combine(Service, _instanceProducer, Lifestyle);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Lifestyle);
        }

        /// <summary>
        /// Instance producer
        /// </summary>
        /// <returns>Built component</returns>
        public object InstanceProducer()
        {
            _instance ??= _instanceProducer();
            return _instance;
        }
    }
}