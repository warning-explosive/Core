namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class OpenGenericTestServiceImpl<T> : IOpenGenericTestService<T>
    {
        public T Do(T param) => param;
    }
}