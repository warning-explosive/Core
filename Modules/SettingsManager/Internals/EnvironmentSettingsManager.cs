namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class EnvironmentSettingsManager : ISettingsManager<EnvironmentSettings>
    {
        /// <inheritdoc />
        public Task<EnvironmentSettings> Get()
        {
            var all = new EnvironmentSettings(All().ToList());

            return Task.FromResult(all);
        }

        /// <inheritdoc />
        public Task Set(EnvironmentSettings value)
        {
            throw new InvalidOperationException("Setting environment variables is prohibited");
        }

        internal EnvironmentSettingsEntry Get(string entryKey)
        {
            var entryValue = Environment
                            .GetEnvironmentVariable(entryKey, EnvironmentVariableTarget.Process)
                            .EnsureNotNull($"Environment settings with key '{entryKey}' must be represented in process environment block");

            return new EnvironmentSettingsEntry(entryKey, entryValue);
        }

        private static IEnumerable<EnvironmentSettingsEntry> All()
        {
            foreach (DictionaryEntry? entry in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                if (entry.HasValue)
                {
                    yield return new EnvironmentSettingsEntry(entry.Value.Key.ToString(), entry.Value.Value.ToString());
                }
            }
        }
    }
}