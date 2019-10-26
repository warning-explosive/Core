namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        string Do();
    }
}