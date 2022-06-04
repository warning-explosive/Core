namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [UnregisteredComponent]
    internal class BaseUnregisteredService : IUnregisteredService,
                                             IResolvable<IUnregisteredService>
    {
    }
}