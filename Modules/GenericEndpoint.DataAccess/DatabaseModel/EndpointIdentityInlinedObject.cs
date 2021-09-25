namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record EndpointIdentityInlinedObject : IInlinedObject
    {
        public EndpointIdentityInlinedObject(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        public string LogicalName { get; private init; }

        public string InstanceName { get; private init; }
    }
}