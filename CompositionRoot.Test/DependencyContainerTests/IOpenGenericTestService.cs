namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Abstractions;

    public interface IOpenGenericTestService<T> : IResolvable
    {
        T Do(T param);
    }
}