namespace SpaceEngineers.Core.StatisticsEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// GetEndpointStatistics query
    /// </summary>
    [OwnedBy(StatisticsEndpointIdentity.LogicalName)]
    public class GetEndpointStatistics : IIntegrationQuery<EndpointStatisticsReply>
    {
        /// <summary>
        /// GetEndpointStatistics
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        public GetEndpointStatistics(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
        }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; }
    }
}