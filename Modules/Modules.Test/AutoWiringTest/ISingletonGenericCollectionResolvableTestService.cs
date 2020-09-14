namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;

    internal interface ISingletonGenericCollectionResolvableTestService<T> : ICollectionResolvable<ISingletonGenericCollectionResolvableTestService<T>>
    {
    }
}