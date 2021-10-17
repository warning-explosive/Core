namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record EndpointIdentity : IInlinedObject
    {
        public EndpointIdentity(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        public string LogicalName { get; private init; }

        public string InstanceName { get; private init; }

        public static implicit operator EndpointIdentity(Contract.EndpointIdentity endpointIdentity)
        {
            return new EndpointIdentity(endpointIdentity.LogicalName, endpointIdentity.InstanceName);
        }
    }
}