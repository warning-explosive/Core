namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(DecorableServiceDecorator2))]
    internal class DecorableServiceDecorator1 : IDecorableService,
                                                IDecorableServiceDecorator
    {
        public DecorableServiceDecorator1(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }

        public IDecorableService Decoratee { get; }
    }
}