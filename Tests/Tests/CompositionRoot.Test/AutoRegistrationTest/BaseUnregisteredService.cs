namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [UnregisteredComponent]
    internal class BaseUnregisteredService : IUnregisteredService,
                                             IResolvable<IUnregisteredService>
    {
    }
}