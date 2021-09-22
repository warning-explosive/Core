namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using Core.DataAccess.Api.DatabaseEntity;

    internal class EndpointIdentityInlinedObject : IInlinedObject
    {
        public EndpointIdentityInlinedObject(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        public string LogicalName { get; }

        public string InstanceName { get; }
    }
}