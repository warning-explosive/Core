namespace SpaceEngineers.Core.AuthEndpoint.Contract
{
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Create user command
    /// </summary>
    [OwnedBy(Identity.LogicalName)]
    [Feature(Features.Authentication)]
    public record CreateUser : IIntegrationCommand
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
        public string Username { get; init; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; init; }
    }
}