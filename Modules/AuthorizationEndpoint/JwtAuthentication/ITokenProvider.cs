namespace SpaceEngineers.Core.AuthorizationEndpoint.JwtAuthentication
{
    using System;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// ITokenProvider
    /// </summary>
    public interface ITokenProvider : IResolvable
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