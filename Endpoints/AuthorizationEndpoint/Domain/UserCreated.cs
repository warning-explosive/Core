namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class UserCreated : BaseDomainEvent<User, UserCreated>
    {
        public UserCreated(
            Guid aggregateId,
            long index,
            DateTime timestamp,
            string username,
            string salt,
            string passwordHash)
            : base(aggregateId, index, timestamp)
        {
            Username = username;
            Salt = salt;
            PasswordHash = passwordHash;
        }

        public string Username { get; private init; }

        public string Salt { get; private init; }

        public string PasswordHash { get; private init; }
    }
}