namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        IIndependentTestService IndependentTestService { get; }
    }
}