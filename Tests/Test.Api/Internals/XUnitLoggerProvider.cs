namespace SpaceEngineers.Core.Test.Api.Internals
{
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    internal class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitConsoleLogger(_testOutputHelper);
        }
    }
}