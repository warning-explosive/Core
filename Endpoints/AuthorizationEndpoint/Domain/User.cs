namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using GenericDomain.Api.Abstractions;

    internal class User : BaseAggregate<User>,
                          IAggregate<User>,
                          IHasDomainEvent<User, UserCreated>
    {
        public User(IEnumerable<IDomainEvent<User>> events)
            : base(events)
        {
        }

        public User(string username, string rawPassword)
            : base(Array.Empty<IDomainEvent<User>>())
        {
            Username = username;
            Salt = SecurityExtensions.GenerateSalt();
            PasswordHash = rawPassword.GenerateSaltedHash(Salt);

            PopulateEvent(new UserCreated(Id, NextDomainEventIndex(), DateTime.UtcNow, username, Salt, PasswordHash));
        }

        private string Username { get; set; } = default!;

        private string PasswordHash { get; set; } = default!;

        private string Salt { get; set; } = default!;

        public bool CheckPassword(string password)
        {
            return password
               .GenerateSaltedHash(Salt)
               .Equals(PasswordHash, StringComparison.Ordinal);
        }

        public void Apply(UserCreated domainEvent)
        {
            Id = domainEvent.AggregateId;
            Username = domainEvent.Username;
            Salt = domainEvent.Salt;
            PasswordHash = domainEvent.PasswordHash;
        }
    }
}