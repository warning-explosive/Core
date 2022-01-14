namespace SpaceEngineers.Core.AuthorizationEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// User authorization result
    /// </summary>
    public class UserAuthorizationResult : IIntegrationReply
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
        public string Username { get; }

        /// <summary>
        /// Authorization token
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Authorization details
        /// </summary>
        public string Details { get; }
    }
}