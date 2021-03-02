namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;

    internal interface ISingletonGenericCollectionResolvableTestService<T> : ICollectionResolvable<ISingletonGenericCollectionResolvableTestService<T>>
    {
    }
}