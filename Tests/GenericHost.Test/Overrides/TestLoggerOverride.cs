namespace SpaceEngineers.Core.GenericHost.Test.Overrides
{
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

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