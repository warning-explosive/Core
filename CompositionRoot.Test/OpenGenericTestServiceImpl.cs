namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    public class OpenGenericTestServiceImpl<T> : IOpenGenericTestService<T>
    {
        public T Do(T param) => param;
    }
}