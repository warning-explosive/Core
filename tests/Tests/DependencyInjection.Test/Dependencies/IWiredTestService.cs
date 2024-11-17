namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

internal interface IWiredTestService
{
    IIndependentTestService IndependentTestService { get; }
}