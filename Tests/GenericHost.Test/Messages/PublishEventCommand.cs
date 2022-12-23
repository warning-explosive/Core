namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    [Feature(nameof(Test))]
    internal record PublishEventCommand : IIntegrationCommand
    {
        public PublishEventCommand(int id)
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