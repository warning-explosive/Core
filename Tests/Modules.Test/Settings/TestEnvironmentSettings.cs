namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;

    internal class TestEnvironmentSettings : ITestSettings
    {
        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public string? String { get; set; }

        public DateTime Date { get; set; }
    }
}