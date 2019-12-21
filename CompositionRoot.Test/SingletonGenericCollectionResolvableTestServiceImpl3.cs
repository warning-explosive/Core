namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl3<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}