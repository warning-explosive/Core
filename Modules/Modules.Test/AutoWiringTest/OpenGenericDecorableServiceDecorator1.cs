namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
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