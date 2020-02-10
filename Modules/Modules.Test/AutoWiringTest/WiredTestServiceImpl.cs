namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WiredTestServiceImpl : IWiredTestService
    {
        public WiredTestServiceImpl(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }

        public IIndependentTestService IndependentTestService { get; }
    }
}