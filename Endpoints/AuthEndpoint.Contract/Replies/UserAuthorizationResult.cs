namespace SpaceEngineers.Core.AuthEndpoint.Contract.Replies
{
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// User authorization result
    /// </summary>
    public record UserAuthorizationResult : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="details">Authorization details</param>
        /// <param name="token">Authorization token</param>
        public UserAuthorizationResult(
            string username,
            string token,
            string details)
        {
            Username = username;
            Token = token;
            Details = details;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// Authorization token
        /// </summary>
        public string Token { get; init; }

        /// <summary>
        /// Authorization details
        /// </summary>
        public string Details { get; init; }
    }
}