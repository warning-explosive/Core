namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Deduplication
{
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    /// <summary>
    /// EndpointIdentity
    /// </summary>
    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    public record EndpointIdentity : IInlinedObject
    {
        /// <summary> .cctor </summary>
        /// <param name="logicalName">LogicalName</param>
        /// <param name="instanceName">InstanceName</param>
        public EndpointIdentity(string logicalName, string instanceName)
        {
            LogicalName = logicalName;
            InstanceName = instanceName;
        }

        /// <summary>
        /// LogicalName
        /// </summary>
        public string LogicalName { get; init; }

        /// <summary>
        /// InstanceName
        /// </summary>
        public string InstanceName { get; init; }

        /// <summary>
        /// implicitly converts Contract.EndpointIdentity to Deduplication.EndpointIdentity
        /// </summary>
        /// <param name="endpointIdentity">Contract.EndpointIdentity</param>
        /// <returns>Deduplication.EndpointIdentity</returns>
        [SuppressMessage("Analysis", "CA2225", Justification = "desired name")]
        public static implicit operator EndpointIdentity(Contract.EndpointIdentity endpointIdentity)
        {
            return new EndpointIdentity(endpointIdentity.LogicalName, endpointIdentity.InstanceName);
        }
    }
}