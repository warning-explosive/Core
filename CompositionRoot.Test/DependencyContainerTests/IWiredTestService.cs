namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        string Do();
    }
}