namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    internal class Query : IIntegrationQuery<Reply>
    {
        public Query(int id)
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