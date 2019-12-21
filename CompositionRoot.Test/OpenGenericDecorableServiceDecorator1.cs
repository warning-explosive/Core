namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(OpenGenericDecorableServiceDecorator2<>))]
    internal class OpenGenericDecorableServiceDecorator1<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                              IDecorator<IOpenGenericDecorableService<T>>
    {
        public OpenGenericDecorableServiceDecorator1(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}