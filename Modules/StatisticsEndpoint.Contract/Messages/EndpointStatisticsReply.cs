namespace SpaceEngineers.Core.StatisticsEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// EndpointStatisticsReply
    /// </summary>
    public class EndpointStatisticsReply : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        public EndpointStatisticsReply(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
        }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; }
    }
}