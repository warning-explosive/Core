namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    internal class RequestQueryCommand : IIntegrationCommand
    {
        public RequestQueryCommand(int id)
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