namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
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