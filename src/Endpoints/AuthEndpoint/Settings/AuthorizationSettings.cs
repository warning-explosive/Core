namespace SpaceEngineers.Core.AuthEndpoint.Settings
{
    using System;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    /// <summary>
    /// Authorization settings
    /// </summary>
    public class AuthorizationSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public AuthorizationSettings()
        {
            TokenExpirationMinutesTimeout = 5;
        }

        /// <summary>
        /// Authorization token expiration timeout (minutes)
        /// </summary>
        public uint TokenExpirationMinutesTimeout { get; init; }

        /// <summary>
        /// Authorization token expiration timeout (minutes)
        /// </summary>
        public TimeSpan TokenExpirationTimeout => TimeSpan.FromMinutes(TokenExpirationMinutesTimeout);
    }
}