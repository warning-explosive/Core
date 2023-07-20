namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using Basics;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// AuthExtensions
    /// </summary>
    public static class AuthExtensions
    {
        /// <summary>
        /// Gets authorization token
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Authorization token</returns>
        public static string? GetAuthorizationToken(this HttpContext context)
        {
            var token = context.User.FindFirst(ClaimTypes.Authentication)?.Value;

            if (!token.IsNullOrWhiteSpace())
            {
                return token;
            }

            var schema = AuthenticationHeaderValue
                .Parse(context.Request.Headers.Authorization)
                .Scheme;

            return schema.Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)
                ? context.Request.Headers.Authorization.ToString()[schema.Length..].Trim()
                : default;
        }

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