namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using Basics.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(DecorableServiceDecorator3))]
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