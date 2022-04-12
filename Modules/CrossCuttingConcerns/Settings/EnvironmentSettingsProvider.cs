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

    [Component(EnLifestyle.Singleton)]
    internal class EnvironmentSettingsProvider : ISettingsProvider<EnvironmentSettings>,
                                                 IResolvable<ISettingsProvider<EnvironmentSettings>>
    {
        public Task<EnvironmentSettings> Get(CancellationToken token)
        {
            var all = new EnvironmentSettings(All().ToList());

            return Task.FromResult(all);
        }

        public Task Set(EnvironmentSettings value, CancellationToken token)
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