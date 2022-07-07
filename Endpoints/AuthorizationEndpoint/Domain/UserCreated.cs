namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using GenericDomain.Api.Abstractions;

    internal class UserCreated : IDomainEvent<User>
    {
        public UserCreated(
            string username,
            string salt,
            string passwordHash)
        {
            Username = username;
            Salt = salt;
            PasswordHash = passwordHash;
        }

        public string Username { get; }

        public string Salt { get; }

        public string PasswordHash { get; }

        public void Apply(User aggregate)
        {
            // TODO: #172 - fill aggregate
            /*
            aggregate.Username = Username;
            aggregate.Salt = Salt;
            aggregate.PasswordHash = PasswordHash;
            */
        }
    }
}