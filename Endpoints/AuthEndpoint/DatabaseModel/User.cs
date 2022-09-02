namespace SpaceEngineers.Core.AuthEndpoint.DatabaseModel
{
    using System;
    using System.Net;
    using DataAccess.Api.Model;

    /// <summary>
    /// User
    /// </summary>
    [Index(nameof(Username), Unique = true)]
    [Schema(nameof(Authorization))]
    public record User : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="username">Username</param>
        public User(
            Guid primaryKey,
            string username)
            : base(primaryKey)
        {
            Username = username;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }
    }
}