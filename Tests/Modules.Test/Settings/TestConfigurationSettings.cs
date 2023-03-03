namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;
    using CrossCuttingConcerns.Settings;

    internal class TestConfigurationSettings : ISettings
    {
        public int Int { get; init; }

        public decimal Decimal { get; init; }

        public string? String { get; init; }

        public DateTime Date { get; init; }

        public TestConfigurationSettings? Reference { get; init; }

        public ICollection<TestConfigurationSettings>? Collection { get; init; }

        public IDictionary<string, TestConfigurationSettings>? Dictionary { get; init; }
    }
}