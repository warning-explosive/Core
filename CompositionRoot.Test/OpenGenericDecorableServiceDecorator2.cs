namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Extensions.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    internal class OpenGenericDecorableServiceDecorator2<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                              IDecorator<IOpenGenericDecorableService<T>>
    {
        public IOpenGenericDecorableService<T> Decoratee { get; }

        public OpenGenericDecorableServiceDecorator2(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }
    }
}