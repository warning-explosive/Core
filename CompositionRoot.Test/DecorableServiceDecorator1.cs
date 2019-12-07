namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class DecorableServiceDecorator1 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public DecorableServiceDecorator1(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }

        public IDecorableService Decoratee { get; }
    }
}