namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;

    public interface IOpenGenericTestService<T> : IResolvable
    {
        T Do(T param);
    }
}