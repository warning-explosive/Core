namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using Basics.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

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