namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    internal class DecorableServiceDecorator2 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public IDecorableService Decoratee { get; }

        public DecorableServiceDecorator2(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }
    }
}