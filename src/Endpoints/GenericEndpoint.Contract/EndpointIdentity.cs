namespace SpaceEngineers.Core.GenericEndpoint.Contract
{
    using System;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    /// <summary>
    /// EndpointIdentity
    /// </summary>
    [ManuallyRegisteredComponent("Is created manually during endpoint's DependencyContainer initialization")]
    public class EndpointIdentity : IEquatable<EndpointIdentity>,
                                    ISafelyEquatable<EndpointIdentity>,
                                    IResolvable<EndpointIdentity>
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public EndpointIdentity()
        {
            LogicalName = default!;
            InstanceName = default!;
            Version = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="logicalName">Endpoint logical name</param>
        /// <param name="assembly">Assembly</param>
        public EndpointIdentity(string logicalName, Assembly assembly)
        {
            LogicalName = logicalName;
            InstanceName = Guid.NewGuid().ToString();
            Assembly = assembly;
            Version = assembly.GetAssemblyVersion();
        }

        /// <summary>
        /// Endpoint logical name
        /// </summary>
        public string LogicalName { get; init; }

        /// <summary>
        /// Endpoint instance name
        /// </summary>
        public string InstanceName { get; init; }

        /// <summary>
        /// Endpoint version
        /// </summary>
        [JsonIgnore]
        public Assembly? Assembly { get; init; }

        /// <summary>
        /// Endpoint version
        /// </summary>
        public string Version { get; init; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EndpointIdentity</param>
        /// <param name="right">Right EndpointIdentity</param>
        /// <returns>equals</returns>
        public static bool operator ==(EndpointIdentity? left, EndpointIdentity? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EndpointIdentity</param>
        /// <param name="right">Right EndpointIdentity</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EndpointIdentity? left, EndpointIdentity? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(EndpointIdentity other)
        {
            return LogicalName.Equals(other.LogicalName, StringComparison.OrdinalIgnoreCase)
                   && InstanceName.Equals(other.InstanceName, StringComparison.OrdinalIgnoreCase)
                   && Version.Equals(other.Version, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool Equals(EndpointIdentity? other)
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
                LogicalName.GetHashCode(StringComparison.OrdinalIgnoreCase),
                InstanceName.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Version.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{LogicalName}:{InstanceName}:{Version}";
        }
    }
}