namespace SpaceEngineers.Core.AuthorizationEndpoint.DatabaseModel
{
    using System;
    using System.Net;
    using DataAccess.Api.Model;

    [Index(nameof(Username), Unique = true)]
    [Schema(nameof(Authorization))]
    internal record User : BaseDatabaseEntity<Guid>
    {
        public User(
            Guid primaryKey,
            string username)
            : base(primaryKey)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}