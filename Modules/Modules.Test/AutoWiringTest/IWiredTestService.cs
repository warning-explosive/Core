namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        IIndependentTestService IndependentTestService { get; }
    }
}