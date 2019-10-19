namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WiredTestServiceImpl : IWiredTestService
    {
        public WiredTestServiceImpl(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }

        public IIndependentTestService IndependentTestService { get; }

        public string Do() => nameof(WiredTestServiceImpl) + " => " + IndependentTestService.Do();
    }
}