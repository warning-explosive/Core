namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class OpenGenericDecorableServiceDecorator3<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                              IDecorator<IOpenGenericDecorableService<T>>
    {
        public OpenGenericDecorableServiceDecorator3(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}