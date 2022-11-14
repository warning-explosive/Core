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
            return Disposable.Create(state, _ => { });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = exception != null
                ? $"{state} | {exception}"
                : state.ToString();

            _output.WriteLine($"[{logLevel}] {eventId} -> {message}");
        }
    }
}