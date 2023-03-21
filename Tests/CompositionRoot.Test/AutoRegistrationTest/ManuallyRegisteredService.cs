namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DependencyContainerOverridesTest.OverrideManuallyRegisteredComponentTest))]
    internal class ManuallyRegisteredService : IManuallyRegisteredService,
                                               IResolvable<IManuallyRegisteredService>,
                                               IResolvable<ManuallyRegisteredService>
    {
    }
}