namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Abstractions;

    internal interface IIndependentTestService : IResolvable
    {
        string Do();
    }
}