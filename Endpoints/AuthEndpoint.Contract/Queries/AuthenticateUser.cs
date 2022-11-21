namespace SpaceEngineers.Core.AuthEndpoint.Contract.Queries
{
    using Replies;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Authenticate user query
    /// </summary>
    [OwnedBy(AuthEndpointIdentity.LogicalName)]
    public record AuthenticateUser : IIntegrationQuery<UserAuthenticationResult>
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public AuthenticateUser(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; init; }
    }
}