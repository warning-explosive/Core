namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    public class SingletonTestServiceImpl : ISingletonTestService
    {
        public string Do() => nameof(SingletonTestServiceImpl);
    }
}