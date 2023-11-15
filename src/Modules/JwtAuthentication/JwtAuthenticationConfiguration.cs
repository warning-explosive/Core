namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Jwt authentication configuration
    /// </summary>
    [ManuallyRegisteredComponent("Requires injection of private key secret into this implementation at startup")]
    public class JwtAuthenticationConfiguration : IResolvable<JwtAuthenticationConfiguration>
    {
        /// <summary> .cctor </summary>
        /// <param name="issuer">Issuer</param>
        /// <param name="audience">Audience</param>
        /// <param name="privateKey">Private key</param>
        public JwtAuthenticationConfiguration(
            string issuer,
            string audience,
            string privateKey)
        {
            Issuer = issuer;
            Audience = audience;
            SecurityKey = new SymmetricSecurityKey(Convert.FromBase64String(privateKey));
        }

        /// <summary>
        /// Issuer
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// Audience
        /// </summary>
        public string Audience { get; }

        /// <summary>
        /// Security key
        /// </summary>
        public SymmetricSecurityKey SecurityKey { get; }

        /// <summary>
        /// Gets token validation parameters
        /// </summary>
        /// <returns>TokenValidationParameters</returns>
        public TokenValidationParameters TokenValidationParameters => new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKey,

            ValidateIssuer = true,
            ValidIssuer = Issuer,

            ValidateAudience = true,
            ValidAudience = Audience,

            RequireExpirationTime = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}