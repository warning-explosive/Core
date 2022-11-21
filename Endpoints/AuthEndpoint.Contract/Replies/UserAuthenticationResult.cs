namespace SpaceEngineers.Core.AuthEndpoint.Contract.Replies
{
    using GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// User authentication result
    /// </summary>
    public record UserAuthenticationResult : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="token">Authorization token</param>
        public UserAuthenticationResult(
            string username,
            string token)
        {
            Username = username;
            Token = token;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// Authorization token
        /// </summary>
        public string Token { get; init; }
    }
}