namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using Utilities.Services.Interfaces;

    public interface ISingletonTestService : IResolvable
    {
        string Do();
    }
}