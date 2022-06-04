namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using Basics.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(OpenGenericDecorableServiceDecorator2<>))]
    internal class OpenGenericDecorableServiceDecorator1<T> : IOpenGenericDecorableService<T>,
                                                              IOpenGenericDecorableServiceDecorator<T>
    {
        public OpenGenericDecorableServiceDecorator1(IOpenGenericDecorableService<T> decorateee)
        {
            Decoratee = decorateee;
        }

        public IOpenGenericDecorableService<T> Decoratee { get; }
    }
}