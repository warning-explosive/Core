namespace SpaceEngineers.Core.AuthorizationEndpoint.DatabaseModel
{
    using System;
    using DataAccess.Api.Model;

    [Index(nameof(Username), Unique = true)]
    [Index(nameof(Salt), Unique = true)]
    internal record User : BaseDatabaseEntity<Guid>
    {
        public User(
            Guid primaryKey,
            string username,
            string passwordHash,
            string salt)
            : base(primaryKey)
        {
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
        }

        public string Username { get; private init; }

        public string PasswordHash { get; private init; }

        public string Salt { get; private init; }
    }
}