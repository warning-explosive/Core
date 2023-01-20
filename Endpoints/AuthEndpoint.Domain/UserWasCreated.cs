namespace SpaceEngineers.Core.AuthEndpoint.Domain
{
    using System;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// UserWasCreated
    /// </summary>
    public record UserWasCreated : IDomainEvent<User>
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="username">Username</param>
        /// <param name="salt">Salt</param>
        /// <param name="passwordHash">Password hash</param>
        public UserWasCreated(
            Guid aggregateId,
            Username username,
            string salt,
            string passwordHash)
        {
            AggregateId = aggregateId;
            Username = username;
            Salt = salt;
            PasswordHash = passwordHash;
        }

        /// <summary>
        /// AggregateId
        /// </summary>
        public Guid AggregateId { get; init; }

        /// <summary>
        /// Username
        /// </summary>
        public Username Username { get; init; }

        /// <summary>
        /// Salt
        /// </summary>
        public string Salt { get; init; }

        /// <summary>
        /// Password hash
        /// </summary>
        public string PasswordHash { get; init; }
    }
}