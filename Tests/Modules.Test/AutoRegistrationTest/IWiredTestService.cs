namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    internal interface IWiredTestService
    {
        IIndependentTestService IndependentTestService { get; }
    }
}