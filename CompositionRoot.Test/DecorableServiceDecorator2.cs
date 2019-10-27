namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    internal class DecorableServiceDecorator2 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public IDecorableService Decoratee { get; }

        internal DecorableServiceDecorator2(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }
    }
}