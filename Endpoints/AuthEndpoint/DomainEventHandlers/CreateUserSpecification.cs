namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    internal class CreateUserSpecification
    {
        public CreateUserSpecification(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }

        public string Password { get; }
    }
}