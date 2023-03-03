namespace SpaceEngineers.Core.GenericEndpoint.Host.Settings
{
    using System;
    using System.IO;
    using Basics;
    using Contract;
    using Microsoft.Extensions.Configuration;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [ManuallyRegisteredComponent("settings configuration")]
    internal class GenericEndpointAppSettingsJsonSettingsProvider<TSettings> : ISettingsProvider<TSettings>,
                                                                               IResolvable<ISettingsProvider<TSettings>>,
                                                                               ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IConfigurationRoot _configuration;

        public GenericEndpointAppSettingsJsonSettingsProvider(
            EndpointIdentity endpointIdentity,
            SettingsDirectoryProvider settingsDirectoryProvider)
        {
            _endpointIdentity = endpointIdentity;

            var directory = settingsDirectoryProvider.SettingsDirectory;

            if (!directory.Exists)
            {
                throw new InvalidOperationException($"Settings directory {directory.FullName} doesn't exists");
            }

            var file = Path.ChangeExtension(Path.Combine(directory.FullName, "appsettings"), "json").AsFileInfo();

            if (!file.Exists)
            {
                throw new InvalidOperationException($"Settings file {file.FullName} doesn't exists");
            }

            _configuration = new ConfigurationBuilder()
                .AddJsonFile(file.FullName)
                .Build();
        }

        /// <inheritdoc />
        public TSettings Get()
        {
            var section = _configuration.GetSection($"Endpoints:{_endpointIdentity.LogicalName}:{typeof(TSettings).Name}");

            return section.Exists()
                ? section.Get<TSettings>() !
                : new TSettings();
        }
    }
}