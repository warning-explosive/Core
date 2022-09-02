namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [After(typeof(DecorableServiceDecorator3))]
    internal class DecorableServiceDecorator2 : IDecorableService,
                                                IDecorableServiceDecorator
    {
        public DecorableServiceDecorator2(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }

        public IDecorableService Decoratee { get; }
    }
}