namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Json;

    /// <summary>
    /// EnvironmentSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    [Component(EnLifestyle.Singleton)]
    public class EnvironmentSettingsProvider<TSettings> : ISettingsProvider<TSettings>,
                                                          IResolvable<EnvironmentSettingsProvider<TSettings>>,
                                                          ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly ISettingsScopeProvider _settingsScopeProvider;
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary> .cctor </summary>
        /// <param name="settingsScopeProvider">ISettingsScopeProvider</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        public EnvironmentSettingsProvider(
            ISettingsScopeProvider settingsScopeProvider,
            IJsonSerializer jsonSerializer)
        {
            _settingsScopeProvider = settingsScopeProvider;
            _jsonSerializer = jsonSerializer;
        }

        /// <inheritdoc />
        public Task<TSettings> Get(CancellationToken token)
        {
            var entry = TryGetValue(typeof(TSettings).Name, _settingsScopeProvider.Scope);

            var settings = entry != null && !entry.Value.IsNullOrEmpty()
                ? _jsonSerializer.DeserializeObject<TSettings>(entry.Value)
                : new TSettings();

            return Task.FromResult(settings);
        }

        internal static EnvironmentSettingsEntry? TryGetValue(string entryKey, string? scope)
        {
            var key = scope != null && !scope.IsNullOrEmpty()
                ? string.Join("__", scope, entryKey)
                : entryKey;

            return All().SingleOrDefault(entry => entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                ?? All().SingleOrDefault(entry => entry.Key.Equals(entryKey, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<EnvironmentSettingsEntry> All()
        {
            foreach (DictionaryEntry? entry in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                yield return new EnvironmentSettingsEntry(entry.Value.Key.ToString(), entry.Value.Value.ToString());
            }
        }

        internal class EnvironmentSettingsEntry
        {
            public EnvironmentSettingsEntry(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; }

            public string Value { get; }
        }
    }
}