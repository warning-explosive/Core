namespace SpaceEngineers.Core.Test.Api.Internals
{
    using System;
    using Basics.Primitives;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    internal class XUnitConsoleLogger : ILogger
    {
        private readonly ITestOutputHelper _output;

        public XUnitConsoleLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return Disposable.Create(state, static _ => { });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _output.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
    }
}