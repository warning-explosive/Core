namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    [Feature(TestFeatures.Test)]
    internal record Request : IIntegrationRequest<Reply>
    {
        public Request(int id)
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