namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
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