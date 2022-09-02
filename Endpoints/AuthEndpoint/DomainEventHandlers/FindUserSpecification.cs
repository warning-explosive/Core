namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using GenericDomain.Api.Abstractions;

    internal class FindUserSpecification : IAggregateSpecification
    {
        public FindUserSpecification(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}