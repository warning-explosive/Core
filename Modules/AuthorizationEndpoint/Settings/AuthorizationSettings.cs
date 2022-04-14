namespace SpaceEngineers.Core.AuthorizationEndpoint.Settings
{
    using System;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// Authorization settings
    /// </summary>
    public class AuthorizationSettings : ISettings
    {
        /// <summary>
        /// Authorization token expiration timeout (minutes)
        /// </summary>
        public uint TokenExpirationMinutesTimeout { get; set; }

        /// <summary>
        /// Authorization token expiration timeout (minutes)
        /// </summary>
        public TimeSpan TokenExpirationTimeout => TimeSpan.FromMinutes(TokenExpirationMinutesTimeout);
    }
}