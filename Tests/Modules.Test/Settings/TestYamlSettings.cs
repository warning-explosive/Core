namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;

    internal class TestYamlSettings : ITestSettings
    {
        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }

        public TestYamlSettings? Reference { get; set; }

        public ICollection<TestYamlSettings>? Collection { get; set; }

        public IDictionary<string, TestYamlSettings>? Dictionary { get; set; }
    }
}