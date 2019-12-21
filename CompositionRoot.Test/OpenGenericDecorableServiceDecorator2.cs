namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(OpenGenericDecorableServiceDecorator3<>))]
    internal class OpenGenericDecorableServiceDecorator2<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                              IDecorator<IOpenGenericDecorableService<T>>
    {
        public OpenGenericDecorableServiceDecorator2(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}