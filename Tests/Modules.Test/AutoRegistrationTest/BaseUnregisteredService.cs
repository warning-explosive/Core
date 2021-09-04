namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;

    [UnregisteredComponent]
    internal class BaseUnregisteredService : IUnregisteredService
    {
    }
}