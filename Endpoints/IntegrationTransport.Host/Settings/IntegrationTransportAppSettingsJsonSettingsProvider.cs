namespace SpaceEngineers.Core.IntegrationTransport.Host.Settings
{
    using System;
    using System.IO;
    using Api;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Configuration;

    [ManuallyRegisteredComponent("settings configuration")]
    internal class IntegrationTransportAppSettingsJsonSettingsProvider<TSettings> : ISettingsProvider<TSettings>,
                                                                                    IResolvable<ISettingsProvider<TSettings>>,
                                                                                    ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly TransportIdentity _transportIdentity;
        private readonly IConfigurationRoot _configuration;

        public IntegrationTransportAppSettingsJsonSettingsProvider(
            TransportIdentity transportIdentity,
            SettingsDirectoryProvider settingsDirectoryProvider)
        {
            _transportIdentity = transportIdentity;

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
            var section = _configuration.GetSection($"Transports:{_transportIdentity.Name}:{typeof(TSettings).Name}");

            return section.Exists()
                ? section.Get<TSettings>() !
                : new TSettings();
        }
    }
}