namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ManuallyRegisteredServiceOverride : IManuallyRegisteredService
    {
    }
}