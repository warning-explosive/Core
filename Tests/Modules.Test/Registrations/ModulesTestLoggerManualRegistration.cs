namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using Microsoft.Extensions.Logging;
    using Mocks;
    using Xunit.Abstractions;

    internal class ModulesTestLoggerManualRegistration : IManualRegistration
    {
        private readonly ITestOutputHelper _output;

        public ModulesTestLoggerManualRegistration(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterInstance<ILogger>(new XUnitConsoleLogger(_output));
        }
    }
}