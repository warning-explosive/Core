namespace SpaceEngineers.Core.Modules.Test.SettingsManager
{
    using System;

    internal interface ITestSettings
    {
        int Int { get; set; }

        decimal Decimal { get; set; }

        string? String { get; set; }

        DateTime Date { get; set; }
    }
}