namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using Utilities.Services.Interfaces;

    public interface IOpenGenericTestService<T> : IResolvable
    {
        T Do(T param);
    }
}