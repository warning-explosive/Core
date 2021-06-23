namespace SpaceEngineers.Core.GenericHost.Api
{
    using System;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Transport injection key
        /// </summary>
        public const string TransportInjectionKey = "transport_injection";

        /// <summary>
        /// Check multiple calls
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="key">Method key</param>
        public static void CheckMultipleCalls(this IHostBuilder hostBuilder, string key)
        {
            if (hostBuilder.Properties.ContainsKey(key))
            {
                throw new InvalidOperationException($"Method `{key}` should be used once in order to configure the same host instance");
            }

            hostBuilder.Properties[key] = true;
        }
    }
}