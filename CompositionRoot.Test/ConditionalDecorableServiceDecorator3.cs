namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class ConditionalDecorableServiceDecorator3 : IConditionalDecorableServiceDecorator,
                                                           IDecorator<IConditionalDecorableService>
    {
        public ConditionalDecorableServiceDecorator3(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public IConditionalDecorableService Decoratee { get; }
    }
}