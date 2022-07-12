﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record EndpointIdentity : IInlinedObject
    {
        public EndpointIdentity(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        public string LogicalName { get; init; }

        public string InstanceName { get; init; }

        public static implicit operator EndpointIdentity(Contract.EndpointIdentity endpointIdentity)
        {
            return new EndpointIdentity(endpointIdentity.LogicalName, endpointIdentity.InstanceName);
        }
    }
}