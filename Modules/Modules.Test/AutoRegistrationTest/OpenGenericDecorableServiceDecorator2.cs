namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(OpenGenericDecorableServiceDecorator3<>))]
    internal class OpenGenericDecorableServiceDecorator2<T> : IOpenGenericDecorableService<T>,
                                                              IOpenGenericDecorableServiceDecorator<T>
    {
        public OpenGenericDecorableServiceDecorator2(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}