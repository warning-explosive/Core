namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl3<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}