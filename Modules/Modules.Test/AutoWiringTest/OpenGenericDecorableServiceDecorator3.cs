namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class OpenGenericDecorableServiceDecorator3<T> : IOpenGenericDecorableService<T>,
                                                              IOpenGenericDecorableServiceDecorator<T>
    {
        public OpenGenericDecorableServiceDecorator3(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}