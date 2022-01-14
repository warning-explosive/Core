namespace SpaceEngineers.Core.AuthorizationEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Create user command
    /// </summary>
    [OwnedBy(AuthorizationEndpointIdentity.LogicalName)]
    public class CreateUser : IIntegrationCommand
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public CreateUser(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; }
    }
}