namespace SpaceEngineers.Core.GenericHost.Test.Overrides
{
    using Microsoft.Extensions.Logging;
    using Mocks;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
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