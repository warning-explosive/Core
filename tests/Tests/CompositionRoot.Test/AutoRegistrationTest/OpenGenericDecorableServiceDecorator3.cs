namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

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