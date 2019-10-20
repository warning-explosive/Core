namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;

    public interface ISingletonTestService : IResolvable
    {
        string Do();
    }
}