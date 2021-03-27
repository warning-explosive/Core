namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(SingletonGenericCollectionResolvableTestServiceImpl3<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl2<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}