namespace SpaceEngineers.Core.JwtAuthentication
{
    using System;
    using Basics;
    using CrossCuttingConcerns.Extensions;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Gets JwtAuthenticationConfiguration
        /// </summary>
        /// <param name="authEndpointConfiguration">IConfiguration</param>
        /// <returns>JwtAuthenticationConfiguration</returns>
        public static JwtAuthenticationConfiguration GetJwtAuthenticationConfiguration(
            this IConfiguration authEndpointConfiguration)
        {
            var section = authEndpointConfiguration.GetRequiredSection("Authorization");

            // var privateKey = Convert.ToBase64String(new HMACSHA256().Key)
            var issuer = section.GetRequiredValue<string>("Issuer");
            var audience = section.GetRequiredValue<string>("Audience");
            var privateKey = section.GetRequiredValue<string>("PrivateKey");

            return new JwtAuthenticationConfiguration(issuer, audience, privateKey);
        }

        /// <summary>
        /// Gets AuthEndpointConfiguration
        /// </summary>
        /// <returns>IConfiguration</returns>
        public static IConfiguration GetAuthEndpointConfiguration()
        {
            var fileSystemSettings = Environment
               .GetEnvironmentVariable(nameof(FileSystemSettings))
               .EnsureNotNull("File system directory should be set up before any endpoint declarations");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            var authEndpointConfigurationFilePath = JsonConvert
               .DeserializeObject<FileSystemSettings>(fileSystemSettings, settings)
               .FileSystemSettingsDirectory
               .AsDirectoryInfo()
               .StepInto("AuthEndpoint")
               .GetFile("appsettings", ".json")
               .FullName;

            return new ConfigurationBuilder()
               .AddJsonFile(authEndpointConfigurationFilePath)
               .Build();
        }
    }
}