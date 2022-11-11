namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// UserCreated
    /// </summary>
    public class UserCreated : IDomainEvent<User>
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="username">Username</param>
        /// <param name="salt">Salt</param>
        /// <param name="passwordHash">Password hash</param>
        public UserCreated(
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