namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;

    internal interface ISingletonGenericCollectionResolvableTestService<T> : ICollectionResolvable<ISingletonGenericCollectionResolvableTestService<T>>
    {
    }
}