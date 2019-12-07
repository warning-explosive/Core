namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class DecorableServiceDecorator3 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public DecorableServiceDecorator3(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }

        public IDecorableService Decoratee { get; }
    }
}