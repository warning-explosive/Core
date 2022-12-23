namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
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
        private const string PermissionsClaim = "Permissions";

        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly JwtAuthenticationConfiguration _configuration;

        public JwtTokenProvider(
            JwtSecurityTokenHandler tokenHandler,
            JwtAuthenticationConfiguration configuration)
        {
            _tokenHandler = tokenHandler;
            _configuration = configuration;
        }

        public string GenerateToken(string username, IReadOnlyCollection<string> permissions, TimeSpan expiration)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim(PermissionsClaim, permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(_configuration.SecurityKey, SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = now,
                Expires = now.Add(expiration),
                Issuer = _configuration.Issuer,
                Audience = _configuration.Audience
            };

            var securityToken = _tokenHandler.CreateToken(tokenDescriptor);

            return _tokenHandler.WriteToken(securityToken);
        }

        public string GetUsername(string token)
        {
            var claims = _tokenHandler.ValidateToken(token, _configuration.TokenValidationParameters, out _);

            return claims
                .Identity
                .Name
                .EnsureNotNull("Jwt token claims should contain user name");
        }

        public IReadOnlyCollection<string> GetPermissions(string token)
        {
            var claims = _tokenHandler.ValidateToken(token, _configuration.TokenValidationParameters, out _);

            return claims
                .Claims
                .Where(claim => claim.Type.Equals(PermissionsClaim, StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}