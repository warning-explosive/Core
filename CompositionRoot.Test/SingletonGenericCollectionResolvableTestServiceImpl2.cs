namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [Order(2)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl2<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}