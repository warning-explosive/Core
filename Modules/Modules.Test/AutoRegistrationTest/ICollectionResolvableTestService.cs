namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface ICollectionResolvableTestService : ICollectionResolvable<ICollectionResolvableTestService>
    {
    }
}