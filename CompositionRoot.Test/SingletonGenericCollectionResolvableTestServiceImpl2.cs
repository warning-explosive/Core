namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    [Order(2)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl2<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}