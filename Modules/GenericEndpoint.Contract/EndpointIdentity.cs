namespace SpaceEngineers.Core.GenericEndpoint.Contract
{
    using System;
    using AutoRegistration.Api.Attributes;
    using Basics;

    /// <summary>
    /// EndpointIdentity
    /// </summary>
    [ManuallyRegisteredComponent]
    public class EndpointIdentity : ISafelyEquatable<EndpointIdentity>,
                                    IEquatable<EndpointIdentity>
    {
        /// <summary> .cctor </summary>
        /// <param name="logicalName">Endpoint logical name</param>
        /// <param name="instanceName">Endpoint instance name</param>
        public EndpointIdentity(string logicalName, object instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName.ToString();
        }

        /// <summary>
        /// Endpoint logical name
        /// </summary>
        public string LogicalName { get; }

        /// <summary>
        /// Endpoint instance name
        /// </summary>
        public string InstanceName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{LogicalName} - {InstanceName}";
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
    }
}