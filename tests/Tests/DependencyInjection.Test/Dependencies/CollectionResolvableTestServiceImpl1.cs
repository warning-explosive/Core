namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(CollectionResolvableTestServiceImpl2))]
internal class CollectionResolvableTestServiceImpl1 : ICollectionResolvableTestService
{
}