namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.Api.Exceptions;

    /// <summary>
    /// User
    /// </summary>
    public class User : BaseAggregate<User>,
                        IAggregate<User>,
                        IHasDomainEvent<User, UserWasCreated>
    {
        private readonly HashSet<string> _grantedPermissions = new HashSet<string>(StringComparer.Ordinal);

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

            PopulateEvent(new UserWasCreated(Id, username, _salt, _passwordHash));
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

        /// <summary>
        /// Checks granted user permissions
        /// </summary>
        /// <param name="requiredFeatures">Required features</param>
        /// <returns>True if user has rights for required features</returns>
        public bool CheckAccess(IReadOnlyCollection<string> requiredFeatures)
        {
            /*
             * TODO: #204 - apply permissions model
             *
             * feature -> provided by code, represents cohesive and united pieces of functionality (web controllers, web methods, message handlers)
             *            in other words what principal can perform in system
             *    e.g. -> basket (for regular users in e-commerce app)
             *            historical_lookup (for privileged users in search app)
             *            admin_console (for support staff)
             *            new_amazing_feature_42 (for soft delivery of pieces of functionality)
             *
             * user -> basic representation of principal, authenticated client
             *         don't have personal permissions, should be included in groups
             *
             * group -> logical union of users or other groups, enables to apply permissions for group of principles
             *  e.g. -> regular_users (basket feature in e-commerce app)
             *          privileged_users (historical_lookup feature in search app)
             *          administrators (admin_console feature for support staff)
             *          alpha_testers / feature_42_first_wave / primary_region (new_amazing_feature_42 feature for soft delivery of pieces of functionality)
             */

            _ = requiredFeatures.All(requiredFeature => _grantedPermissions.Contains(requiredFeature));

            /*if (!requiredFeatures.Any())
            {
                throw new InvalidOperationException("Piece of functionality should be included at least in one feature");
            }*/

            return true;
        }

        /// <inheritdoc />
        public void Apply(UserWasCreated domainEvent)
        {
            Id = domainEvent.AggregateId;
            _username = domainEvent.Username;
            _salt = domainEvent.Salt;
            _passwordHash = domainEvent.PasswordHash;
        }
    }
}