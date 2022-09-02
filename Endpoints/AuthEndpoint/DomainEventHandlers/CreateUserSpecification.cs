namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using GenericDomain.Api.Abstractions;

    internal class CreateUserSpecification : IAggregateSpecification
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