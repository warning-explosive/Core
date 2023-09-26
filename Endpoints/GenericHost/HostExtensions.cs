namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Basics;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        /// Builds configured host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="settingsDirectory">Settings directory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHost BuildHost(
            this IHostBuilder hostBuilder,
            DirectoryInfo settingsDirectory)
        {
            // TODO: #225 check for al least 1 endpoint and transport
            hostBuilder.CheckMultipleCalls(nameof(BuildHost));

            return hostBuilder
               .ConfigureAppConfiguration(builder => builder
                   .AddJsonFile(settingsDirectory.GetFile("appsettings", ".json").FullName))
               .ConfigureServices((_, serviceCollection) => serviceCollection
                   .AddSingleton(InitializeFrameworkDependenciesProvider())
                   .AddSingleton(InitializeSettingsDirectoryProvider(settingsDirectory)))
               .Build();

            static Func<IServiceProvider, IFrameworkDependenciesProvider> InitializeFrameworkDependenciesProvider()
            {
                return serviceProvider =>
                {
                    var frameworkDependenciesProvider = new FrameworkDependenciesProvider();
                    frameworkDependenciesProvider.SetServiceProvider(serviceProvider);
                    return frameworkDependenciesProvider;
                };
            }

            static SettingsDirectoryProvider InitializeSettingsDirectoryProvider(DirectoryInfo settingsDirectory)
            {
                return new SettingsDirectoryProvider(settingsDirectory);
            }
        }
    }
}