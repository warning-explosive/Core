namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class WiredTestServiceImpl : IWiredTestService
    {
        public WiredTestServiceImpl(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }

        public IIndependentTestService IndependentTestService { get; }
    }
}