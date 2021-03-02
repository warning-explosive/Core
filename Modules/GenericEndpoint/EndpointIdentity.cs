namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;

    /// <summary>
    /// Endpoint identity
    /// </summary>
    [ManualRegistration]
    public class EndpointIdentity : IResolvable,
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
        public bool Equals(EndpointIdentity? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return LogicalName.Equals(other.LogicalName, StringComparison.OrdinalIgnoreCase)
                   && InstanceName.Equals(other.InstanceName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((EndpointIdentity)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(LogicalName, InstanceName);
        }
    }
}