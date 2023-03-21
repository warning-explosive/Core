namespace SpaceEngineers.Core.JwtAuthentication
{
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Gets JwtAuthenticationConfiguration
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>JwtAuthenticationConfiguration</returns>
        public static JwtAuthenticationConfiguration GetJwtAuthenticationConfiguration(
            this IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection($"Endpoints:{AuthEndpoint.Contract.Identity.LogicalName}:Authorization");

            // var privateKey = Convert.ToBase64String(new HMACSHA256().Key)
            var issuer = section.GetRequiredValue<string>("Issuer");
            var audience = section.GetRequiredValue<string>("Audience");
            var privateKey = section.GetRequiredValue<string>("PrivateKey");

            return new JwtAuthenticationConfiguration(issuer, audience, privateKey);
        }
    }
}