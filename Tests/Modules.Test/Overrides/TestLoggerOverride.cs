namespace SpaceEngineers.Core.Modules.Test.Overrides
{
    using CompositionRoot.Api.Abstractions.Registration;
    using Microsoft.Extensions.Logging;
    using Mocks;
    using Xunit.Abstractions;

    internal class TestLoggerOverride : IComponentsOverride
    {
        private readonly ITestOutputHelper _output;

        public TestLoggerOverride(ITestOutputHelper output)
        {
            _output = output;
        }

        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.OverrideInstance<ILogger>(new XUnitConsoleLogger(_output));
        }
    }
}