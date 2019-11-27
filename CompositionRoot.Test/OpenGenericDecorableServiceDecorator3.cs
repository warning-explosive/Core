namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class OpenGenericDecorableServiceDecorator3<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                              IDecorator<IOpenGenericDecorableService<T>>
    {
        public IOpenGenericDecorableService<T> Decoratee { get; }

        public OpenGenericDecorableServiceDecorator3(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }
    }
}