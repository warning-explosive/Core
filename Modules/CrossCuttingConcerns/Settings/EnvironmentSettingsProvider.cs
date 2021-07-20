namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class EnvironmentSettingsProvider : ISettingsProvider<EnvironmentSettings>
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

        internal static EnvironmentSettingsEntry Get(string entryKey)
        {
            var entryValue = Environment
                            .GetEnvironmentVariable(entryKey, EnvironmentVariableTarget.Process)
                            .EnsureNotNull($"Environment settings with key '{entryKey}' should be represented in process environment block");

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