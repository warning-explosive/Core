namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using IntegrationTransport.Host;

    [OwnedBy(Identity.LogicalName)]
    [Feature(TestFeatures.Test)]
    internal record TransportEvent : IIntegrationEvent
    {
        public TransportEvent(int id)
        {
            Id = id;
        }

        public int Id { get; init; }

        public override string ToString()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}