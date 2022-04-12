namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;
    using CrossCuttingConcerns.Settings;

    internal class TestJsonSettings : ITestSettings, IJsonSettings
    {
        public TestJsonSettings(ITestSettings reference)
        {
            var date = DateTime.MinValue;

            Int = date.Year;
            Decimal = 42.42m;
            String = "Hello world!";
            Date = date;

            Reference = reference;
            Collection = new List<ITestSettings> { reference };
            Dictionary = new Dictionary<string, ITestSettings> { ["First"] = reference };
        }

        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }

        public ITestSettings? Reference { get; set; }

        public ICollection<ITestSettings>? Collection { get; set; }

        public IDictionary<string, ITestSettings>? Dictionary { get; set; }

        internal static TestJsonSettings CreateJsonSettings()
        {
            var inner = new TestYamlSettings();

            return new TestJsonSettings(inner);
        }
    }
}