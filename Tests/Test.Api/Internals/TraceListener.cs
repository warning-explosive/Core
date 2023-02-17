namespace SpaceEngineers.Core.Test.Api.Internals
{
    using Basics;

    internal class TraceListener : System.Diagnostics.TraceListener
    {
        public override bool IsThreadSafe => true;

        public override void Write(string? value)
        {
            if (!value.IsNullOrEmpty())
            {
                TestBase.Local.Value?.Output.WriteLine(value.TrimEnd('\r', '\n'));
            }
        }

        public override void WriteLine(string? value)
        {
            TestBase.Local.Value?.Output.WriteLine(value?.TrimEnd('\r', '\n') ?? string.Empty);
        }
    }
}