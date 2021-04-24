namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    /// <summary>
    /// Endpoint identity
    /// </summary>
    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    public class EndpointIdentity : IResolvable,
                                    ISafelyEquatable<EndpointIdentity>,
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
            return $"{LogicalName}_{InstanceName}";
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