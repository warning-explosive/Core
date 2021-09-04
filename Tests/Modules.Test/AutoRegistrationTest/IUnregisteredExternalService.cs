namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface IUnregisteredExternalService : IExternalResolvable<IUnregisteredExternalService>
    {
    }
}