namespace SpaceEngineers.Core.AuthorizationEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Authorize user query
    /// </summary>
    [OwnedBy(AuthorizationEndpointIdentity.LogicalName)]
    public class AuthorizeUser : IIntegrationQuery<UserAuthorizationResult>
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public AuthorizeUser(string username, string password)
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