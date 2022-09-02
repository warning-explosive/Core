namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface ICollectionResolvableTestService : ICollectionResolvable<ICollectionResolvableTestService>
    {
    }
}