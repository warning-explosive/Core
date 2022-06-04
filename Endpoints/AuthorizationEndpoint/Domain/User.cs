namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using Extensions;
    using GenericDomain.Api.Abstractions;

    internal class User : BaseAggregate
    {
        public User(string username, string rawPassword)
        {
            Username = username;
            Salt = SecurityExtensions.GenerateSalt();
            PasswordHash = rawPassword.GenerateSaltedHash(Salt);

            PopulateEvent(new UserCreated(username, PasswordHash));
        }

        public User(DatabaseModel.User userDatabaseEntity)
        {
            Username = userDatabaseEntity.Username;
            PasswordHash = userDatabaseEntity.PasswordHash;
            Salt = userDatabaseEntity.Salt;
        }

        public string Username { get; }

        public string PasswordHash { get; }

        public string Salt { get; }
    }
}