namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class UserCreated : IDomainEvent<User>
    {
        public UserCreated(
            Guid aggregateId,
            string username,
            string salt,
            string passwordHash)
        {
            AggregateId = aggregateId;
            Username = username;
            Salt = salt;
            PasswordHash = passwordHash;
        }

        public Guid AggregateId { get; }

        public string Username { get; }

        public string Salt { get; }

        public string PasswordHash { get; }

        public void Apply(User aggregate)
        {
            aggregate.Apply(this);
        }
    }
}