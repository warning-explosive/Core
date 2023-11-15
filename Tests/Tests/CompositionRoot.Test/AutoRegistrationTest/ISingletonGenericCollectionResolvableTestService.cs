namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface ISingletonGenericCollectionResolvableTestService<T> : ICollectionResolvable<ISingletonGenericCollectionResolvableTestService<T>>
    {
    }
}