namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using System;
    using CrossCuttingConcerns.Settings;

    internal interface ITestSettings : ISettings
    {
        int Int { get; set; }

        decimal Decimal { get; set; }

        string? String { get; set; }

        DateTime Date { get; set; }
    }
}