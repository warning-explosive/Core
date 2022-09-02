namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;

    /// <summary>
    /// ITokenProvider
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Generates token
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="expiration">Expiration period</param>
        /// <returns>Token</returns>
        string GenerateToken(string username, TimeSpan expiration);

        /// <summary>
        /// Gets user name
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>User name</returns>
        string GetUsername(string token);
    }
}