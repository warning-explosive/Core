namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class DecorableServiceDecorator3 : IDecorableService,
                                                IDecorableServiceDecorator
    {
        public DecorableServiceDecorator3(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }

        public IDecorableService Decoratee { get; }
    }
}