namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ITokenProvider
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Generates token
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="permissions">Permissions</param>
        /// <param name="expiration">Expiration period</param>
        /// <returns>Token</returns>
        string GenerateToken(string username, IReadOnlyCollection<string> permissions, TimeSpan expiration);

        /// <summary>
        /// Gets user name
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>User name</returns>
        string GetUsername(string token);

        /// <summary>
        /// Gets permissions
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Permissions</returns>
        IReadOnlyCollection<string> GetPermissions(string token);
    }
}