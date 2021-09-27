namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    internal class PublishEventCommand : IIntegrationCommand
    {
        public PublishEventCommand(int id)
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