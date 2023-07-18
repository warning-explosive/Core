namespace SpaceEngineers.Core.GenericHost.Api
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Check multiple calls
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="key">Method key</param>
        public static void CheckMultipleCalls(this IHostBuilder hostBuilder, string key)
        {
            var added = false;

            _ = hostBuilder.Properties.GetOrAdd(
                key,
                _ =>
                {
                    added = true;
                    return true;
                });

            if (!added)
            {
                throw new InvalidOperationException($"Method `{key}` should be used once so as to correctly configure the host instance");
            }
        }

        /// <summary>
        /// Gets property value
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Property value</returns>
        public static bool TryGetPropertyValue<T>(
            this IHostBuilder hostBuilder,
            string propertyName,
            [NotNullWhen(true)] out T? value)
        {
            if (hostBuilder.Properties.TryGetValue(propertyName, out var raw)
                && raw is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Sets property value
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void SetPropertyValue<T>(
            this IHostBuilder hostBuilder,
            string key,
            T value)
            where T : notnull
        {
            if (hostBuilder.TryGetPropertyValue<T>(key, out _))
            {
                throw new InvalidOperationException($"Host property value '{key}' has already been set");
            }

            hostBuilder.Properties[key] = value;
        }

        /// <summary>
        /// Appends property value to collection
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void AppendPropertyValue<T>(
            this IHostBuilder hostBuilder,
            string key,
            T value)
        {
            if (!hostBuilder.TryGetPropertyValue<T[]>(key, out var values))
            {
                values = Array.Empty<T>();
            }

            hostBuilder.Properties[key] = values
                .Concat(new[] { value })
                .ToArray();
        }
    }
}