namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Microsoft.IdentityModel.Tokens;

    [Component(EnLifestyle.Singleton)]
    internal class JwtTokenProvider : ITokenProvider,
                                      IResolvable<ITokenProvider>
    {
        private readonly JwtAuthenticationConfiguration _configuration;

        public JwtTokenProvider(JwtAuthenticationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string username, TimeSpan expiration)
        {
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                SigningCredentials = new SigningCredentials(_configuration.SecurityKey, SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = now,
                Expires = now.Add(expiration),
                Issuer = _configuration.Issuer,
                Audience = _configuration.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        public string GetUsername(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = tokenHandler.ValidateToken(token, _configuration.TokenValidationParameters, out _);

            return claims
                .Identity
                .Name
                .EnsureNotNull("Jwt token claims should contain user name");
        }
    }
}