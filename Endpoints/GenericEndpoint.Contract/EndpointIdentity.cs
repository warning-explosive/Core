namespace SpaceEngineers.Core.GenericEndpoint.Contract
{
    using System;
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
        /// <param name="logicalName">Endpoint logical name</param>
        /// <param name="instanceName">Endpoint instance name</param>
        public EndpointIdentity(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        /// <summary>
        /// Endpoint logical name
        /// </summary>
        public string LogicalName { get; }

        /// <summary>
        /// Endpoint instance name
        /// </summary>
        public string InstanceName { get; }

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
                   && InstanceName.Equals(other.InstanceName, StringComparison.OrdinalIgnoreCase);
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
            return HashCode.Combine(LogicalName, InstanceName);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{LogicalName} - {InstanceName}";
        }
    }
}