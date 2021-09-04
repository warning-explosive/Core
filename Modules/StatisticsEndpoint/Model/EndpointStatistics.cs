namespace SpaceEngineers.Core.StatisticsEndpoint.Model
{
    using System.Collections.Generic;
    using GenericDomain.Api;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Contract;

    internal class EndpointStatistics : EntityBase, IAggregate
    {
        public EndpointStatistics(EndpointIdentity endpointIdentity)
        {
            EndpointLogicalName = endpointIdentity.LogicalName;
            EndpointInstanceName = endpointIdentity.InstanceName;

            SuccessfulMessages = new List<MessageInfo>();
            FailedMessages = new List<FailedMessage>();
        }

        public string EndpointLogicalName { get; }

        public string EndpointInstanceName { get; }

        public ICollection<MessageInfo> SuccessfulMessages { get; set; }

        public ICollection<FailedMessage> FailedMessages { get; }
    }
}