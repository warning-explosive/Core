namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class WiredTestService : IWiredTestService
{
    public WiredTestService(IIndependentTestService independentTestService)
    {
        IndependentTestService = independentTestService;
    }

    public IIndependentTestService IndependentTestService { get; }
}