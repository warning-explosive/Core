namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DependencyContainerOverridesTest.OverrideManuallyRegisteredComponentTest))]
    internal class ManuallyRegisteredService : IManuallyRegisteredService
    {
    }
}