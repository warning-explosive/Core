namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(CollectionResolvableTestServiceImpl3))]
internal class CollectionResolvableTestServiceImpl2 : ICollectionResolvableTestService
{
}