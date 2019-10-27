namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    
    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class DecorableServiceDecorator1 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public IDecorableService Decoratee { get; }

        internal DecorableServiceDecorator1(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }
    }
}