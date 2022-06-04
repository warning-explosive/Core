namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class WiredTestService : IWiredTestService,
                                      IResolvable<IWiredTestService>
    {
        public WiredTestService(IIndependentTestService independentTestService)
        {
            IndependentTestService = independentTestService;
        }

        public IIndependentTestService IndependentTestService { get; }
    }
}