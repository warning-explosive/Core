namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(SingletonGenericCollectionResolvableTestServiceImpl2<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl1<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}