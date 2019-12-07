namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [Order(3)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl1<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}