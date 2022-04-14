namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using System.Collections.Generic;

    internal class TestJsonSettings : ITestSettings
    {
        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }

        public TestJsonSettings? Reference { get; set; }

        public ICollection<TestJsonSettings>? Collection { get; set; }

        public IDictionary<string, TestJsonSettings>? Dictionary { get; set; }
    }
}