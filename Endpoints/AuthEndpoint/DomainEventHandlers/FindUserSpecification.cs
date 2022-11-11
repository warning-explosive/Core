namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    internal class FindUserSpecification
    {
        public FindUserSpecification(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}