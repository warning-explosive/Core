namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(SingletonGenericCollectionResolvableTestServiceImpl3<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl2<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}