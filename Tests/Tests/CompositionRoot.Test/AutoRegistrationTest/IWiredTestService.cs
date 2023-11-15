namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    internal interface IWiredTestService
    {
        IIndependentTestService IndependentTestService { get; }
    }
}