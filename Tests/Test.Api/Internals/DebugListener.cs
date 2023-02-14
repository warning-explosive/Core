namespace SpaceEngineers.Core.Test.Api.Internals
{
    using System;
    using Basics;

    internal static class DebugListener
    {
        internal static Action<string> Write { get; } = value =>
        {
            if (!value.IsNullOrEmpty())
            {
                TestBase.Local.Value.Output.WriteLine(value.TrimEnd('\r', '\n'));
            }
        };
    }
}