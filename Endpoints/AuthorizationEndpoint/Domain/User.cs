namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using GenericDomain.Api.Abstractions;

    internal class User : BaseAggregate<User>, IAggregate<User>
    {
        public User(IEnumerable<IDomainEvent<User>> events)
            : base(events)
        {
            // TODO: #172 - default .cctor verification
        }

        public User(string username, string rawPassword)
            : base(Array.Empty<IDomainEvent<User>>())
        {
            Username = username;
            Salt = SecurityExtensions.GenerateSalt();
            PasswordHash = rawPassword.GenerateSaltedHash(Salt);

            PopulateEvent(new UserCreated(username, Salt, PasswordHash));
        }

        private string Username { get; } = default!;

        private string PasswordHash { get; } = default!;

        private string Salt { get; } = default!;

        public bool CheckPassword(string password)
        {
            return password
               .GenerateSaltedHash(Salt)
               .Equals(PasswordHash, StringComparison.Ordinal);
        }
    }
}