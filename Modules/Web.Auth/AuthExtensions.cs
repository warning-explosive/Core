namespace SpaceEngineers.Core.Web.Auth
{
    using System;
    using System.Text;

    /// <summary>
    /// AuthExtensions
    /// </summary>
    public static class AuthExtensions
    {
        /// <summary>
        /// EncodeBasicAuth
        /// </summary>
        /// <param name="credentials">Credentials</param>
        /// <returns>Token</returns>
        public static string EncodeBasicAuth(this (string username, string password) credentials)
        {
            var (username, password) = credentials;

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            return token;
        }

        /// <summary>
        /// DecodeBasicAuth
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Credentials</returns>
        public static (string username, string password) DecodeBasicAuth(this string token)
        {
            var credentialBytes = Convert.FromBase64String(token);

            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

            var username = credentials[0];
            var password = credentials[1];

            return (username, password);
        }
    }
}