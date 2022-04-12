namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;
    using CrossCuttingConcerns.Settings;

    internal class TestYamlSettings : ITestSettings, IYamlSettings
    {
        public TestYamlSettings()
        {
            var date = DateTime.MinValue;

            Int = date.Year;
            Decimal = 42.42m;
            String = "Hello world!";
            Date = date;
        }

        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }

        public TestYamlSettings? Reference { get; set; }

        public ICollection<TestYamlSettings>? Collection { get; set; }

        public IDictionary<string, TestYamlSettings>? Dictionary { get; set; }

        internal static TestYamlSettings CreateYamlSettings()
        {
            var inner = new TestYamlSettings();

            return new TestYamlSettings
                   {
                       Reference = inner,
                       Collection = new List<TestYamlSettings> { inner },
                       Dictionary = new Dictionary<string, TestYamlSettings> { ["First"] = inner }
                   };
        }
    }
}