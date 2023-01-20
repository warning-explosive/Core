namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ConfigurationExtensions
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets required value
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        /// <param name="key">Key</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Required value or exception</returns>
        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
            where T : notnull
        {
            return configuration.GetValue<T>(key)
                ?? throw new InvalidOperationException($"Missing configuration value with key {key}");
        }
    }
}