namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [Dependency(typeof(SingletonGenericCollectionResolvableTestServiceImpl2<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl1<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}