namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Configuration;
    using IConfigurationProvider = CrossCuttingConcerns.Settings.IConfigurationProvider;

    [ManuallyRegisteredComponent("Framework dependency")]
    internal class ConfigurationProvider : IConfigurationProvider,
                                           IResolvable<IConfigurationProvider>
    {
        private readonly ConcurrentDictionary<string, IConfiguration> _configurations =
            new ConcurrentDictionary<string, IConfiguration>(StringComparer.OrdinalIgnoreCase);

        public bool TryGet<TSettings>(string path, [NotNullWhen(true)] out TSettings? settings)
            where TSettings : class, ISettings, new()
        {
            var configuration = _configurations.GetOrAdd(path, BuildConfiguration);

            var section = configuration.GetSection(typeof(TSettings).Name);

            if (section.Exists())
            {
                settings = section.Get<TSettings>();
                return true;
            }

            settings = default;
            return false;
        }

        private static IConfigurationRoot BuildConfiguration(string path)
        {
            return new ConfigurationBuilder()
               .AddJsonFile(path)
               .Build();
        }
    }
}