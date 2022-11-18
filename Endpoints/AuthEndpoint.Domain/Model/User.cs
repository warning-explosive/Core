namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System;
    using System.Linq;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.Api.Exceptions;

    /// <summary>
    /// User
    /// </summary>
    public class User : BaseAggregate<User>,
                        IAggregate<User>,
                        IHasDomainEvent<User, UserCreated>
    {
        private Username _username = default!;

        private string _passwordHash = default!;

        private string _salt = default!;

        /// <summary> .cctor </summary>
        /// <param name="events">Domain events</param>
        public User(IDomainEvent<User>[] events)
            : base(events)
        {
            if (!events.Any())
            {
                throw new DomainInvariantViolationException($"{nameof(User)} should have at least one domain event");
            }
        }

        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="rawPassword">Raw password</param>
        public User(Username username, Password rawPassword)
            : base(Array.Empty<IDomainEvent<User>>())
        {
            _username = username;
            _salt = Password.GenerateSalt();
            _passwordHash = rawPassword.GeneratePasswordHash(_salt);

            PopulateEvent(new UserCreated(Id, username, _salt, _passwordHash));
        }

        /// <summary>
        /// Validates entered password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>True if password is correct</returns>
        public bool CheckPassword(Password password)
        {
            return password
                .GeneratePasswordHash(_salt)
                .Equals(_passwordHash, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public void Apply(UserCreated domainEvent)
        {
            Id = domainEvent.AggregateId;
            _username = domainEvent.Username;
            _salt = domainEvent.Salt;
            _passwordHash = domainEvent.PasswordHash;
        }
    }
}