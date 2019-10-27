namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WiredTestServiceImpl : IWiredTestService
    {
        public IIndependentTestService IndependentTestService { get; }

        public WiredTestServiceImpl(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }
    }
}