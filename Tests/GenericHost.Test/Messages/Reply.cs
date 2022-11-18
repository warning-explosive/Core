namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;

    internal record Reply : IIntegrationReply
    {
        public Reply(int id)
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