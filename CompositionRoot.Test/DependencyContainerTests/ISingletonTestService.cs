namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Abstractions;

    public interface ISingletonTestService : IResolvable
    {
        string Do();
    }
}