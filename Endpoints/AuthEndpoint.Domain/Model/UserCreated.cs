namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// UserCreated
    /// </summary>
    public class UserCreated : BaseDomainEvent<User, UserCreated>
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="index">Index</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="username">Username</param>
        /// <param name="salt">Salt</param>
        /// <param name="passwordHash">Password hash</param>
        public UserCreated(
            Guid aggregateId,
            long index,
            DateTime timestamp,
            Username username,
            string salt,
            string passwordHash)
            : base(aggregateId, index, timestamp)
        {
            Username = username;
            Salt = salt;
            PasswordHash = passwordHash;
        }

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