namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    [Feature(TestFeatures.Test)]
    internal record Event : IIntegrationEvent
    {
        public Event(int id)
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