namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using Utilities.Services.Interfaces;

    internal interface IWiredTestService : IResolvable
    {
        string Do();
    }
}