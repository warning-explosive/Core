namespace SpaceEngineers.Core.IntegrationTransport.Api
{
    using System;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    /// <summary>
    /// TransportIdentity
    /// </summary>
    [ManuallyRegisteredComponent("Is created manually during endpoint's DependencyContainer initialization")]
    public class TransportIdentity : IEquatable<TransportIdentity>,
                                     ISafelyEquatable<TransportIdentity>,
                                     IResolvable<TransportIdentity>
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public TransportIdentity()
        {
            Name = default!;
            Version = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="name">Name</param>
        /// <param name="assembly">Assembly</param>
        public TransportIdentity(
            string name,
            Assembly assembly)
        {
            Name = name;
            Version = assembly.GetAssemblyVersion();
        }

        /// <summary>
        /// Transport name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Endpoint version
        /// </summary>
        public string Version { get; init; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left TransportIdentity</param>
        /// <param name="right">Right TransportIdentity</param>
        /// <returns>equals</returns>
        public static bool operator ==(TransportIdentity? left, TransportIdentity? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left TransportIdentity</param>
        /// <param name="right">Right TransportIdentity</param>
        /// <returns>not equals</returns>
        public static bool operator !=(TransportIdentity? left, TransportIdentity? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(TransportIdentity other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Version.Equals(other.Version, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool Equals(TransportIdentity? other)
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
            return HashCode.Combine(
                Name.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Version.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}:{Version}";
        }
    }
}