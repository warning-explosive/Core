namespace SpaceEngineers.Core.Test.Api.Logging
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal class TestOutputTextWriter : TextWriter
    {
        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void Write(char value)
        {
            TestBase.Local.Value?.Output.WriteLine(value.ToString());
        }

        public override void Write(string? value)
        {
            TestBase.Local.Value?.Output.WriteLine(value?.TrimEnd('\r', '\n') ?? string.Empty);
        }

        public override void WriteLine()
        {
            TestBase.Local.Value?.Output.WriteLine(string.Empty);
        }

        public override void WriteLine(string? value)
        {
            TestBase.Local.Value?.Output.WriteLine(value?.TrimEnd('\r', '\n') ?? string.Empty);
        }

        public override Task WriteAsync(char value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(string? value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync()
        {
            WriteLine();
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string? value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }
    }
}