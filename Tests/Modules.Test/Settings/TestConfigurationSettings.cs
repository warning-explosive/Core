namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;

    internal class TestConfigurationSettings : ITestSettings
    {
        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }

        public TestConfigurationSettings? Reference { get; set; }

        public ICollection<TestConfigurationSettings>? Collection { get; set; }

        public IDictionary<string, TestConfigurationSettings>? Dictionary { get; set; }
    }
}