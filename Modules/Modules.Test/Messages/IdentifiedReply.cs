namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System.Globalization;
    using GenericEndpoint.Contract.Abstractions;

    internal class IdentifiedReply : IIntegrationMessage
    {
        public IdentifiedReply(int id)
        {
            Id = id;
        }

        internal int Id { get; }

        public override string ToString()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}