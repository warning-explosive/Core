namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        IIndependentTestService IndependentTestService { get; }
    }
}