namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface ISingletonGenericCollectionResolvableTestService<T> : ICollectionResolvable<ISingletonGenericCollectionResolvableTestService<T>>
    {
    }
}