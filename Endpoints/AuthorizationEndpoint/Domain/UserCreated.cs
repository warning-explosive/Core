namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using GenericDomain.Api.Abstractions;

    internal class UserCreated : IDomainEvent
    {
        public UserCreated(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        public string Username { get; }

        public string PasswordHash { get; }
    }
}