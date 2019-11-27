namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class DecorableServiceDecorator1 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public IDecorableService Decoratee { get; }

        public DecorableServiceDecorator1(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }
    }
}