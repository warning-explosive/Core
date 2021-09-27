namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    internal class Event : IIntegrationEvent
    {
        public Event(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public override string ToString()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}