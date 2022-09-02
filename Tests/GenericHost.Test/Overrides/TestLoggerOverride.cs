namespace SpaceEngineers.Core.GenericHost.Test.Overrides
{
    using CompositionRoot.Registration;
    using Microsoft.Extensions.Logging;

    internal class TestLoggerOverride : IComponentsOverride
    {
        private readonly ILogger _logger;

        public TestLoggerOverride(ILogger logger)
        {
            _logger = logger;
        }

        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.OverrideInstance<ILogger>(_logger);
        }
    }
}