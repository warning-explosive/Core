namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DependencyContainerOverridesTest.OverrideManuallyRegisteredComponentTest))]
    internal class ManuallyRegisteredService : IManuallyRegisteredService,
                                               IResolvable<IManuallyRegisteredService>,
                                               IResolvable<ManuallyRegisteredService>
    {
    }
}