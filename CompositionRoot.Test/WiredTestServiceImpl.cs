namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WiredTestServiceImpl : IWiredTestService
    {
        internal WiredTestServiceImpl(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }

        internal IIndependentTestService IndependentTestService { get; }

        public string Do() => nameof(WiredTestServiceImpl) + " => " + IndependentTestService.Do();
    }
}