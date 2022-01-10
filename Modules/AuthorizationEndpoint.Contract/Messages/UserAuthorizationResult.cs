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
        /// <param name="result">Authorization result (true if authorization was successful)</param>
        /// <param name="details">Authorization details</param>
        public UserAuthorizationResult(
            string username,
            bool result,
            string? details)
        {
            Username = username;
            Result = result;
            Details = details ?? string.Empty;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Authorization result (true if authorization was successful)
        /// </summary>
        public bool Result { get; }

        /// <summary>
        /// Authorization details
        /// </summary>
        public string Details { get; }
    }
}