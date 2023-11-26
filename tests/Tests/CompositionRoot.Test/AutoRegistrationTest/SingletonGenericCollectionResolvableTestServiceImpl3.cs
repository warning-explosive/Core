namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class SingletonGenericCollectionResolvableTestServiceImpl3<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}