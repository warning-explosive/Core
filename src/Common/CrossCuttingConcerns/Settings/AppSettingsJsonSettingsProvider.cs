namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.IO;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// AppSettingsJsonSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    [Component(EnLifestyle.Singleton)]
    public class AppSettingsJsonSettingsProvider<TSettings> : ISettingsProvider<TSettings>,
                                                              IResolvable<AppSettingsJsonSettingsProvider<TSettings>>,
                                                              ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly IConfigurationRoot _configuration;

        /// <summary> .cctor </summary>
        /// <param name="settingsDirectoryProvider">Settings directory provider</param>
        public AppSettingsJsonSettingsProvider(SettingsDirectoryProvider settingsDirectoryProvider)
        {
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
            var section = _configuration.GetSection(typeof(TSettings).Name);

            return section.Exists()
                ? section.Get<TSettings>() !
                : new TSettings();
        }
    }
}