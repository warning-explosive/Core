namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Extensions.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class DecorableServiceDecorator3 : IDecorableServiceDecorator,
                                                IDecorator<IDecorableService>
    {
        public IDecorableService Decoratee { get; }

        public DecorableServiceDecorator3(IDecorableService decoratorType)
        {
            Decoratee = decoratorType;
        }
    }
}