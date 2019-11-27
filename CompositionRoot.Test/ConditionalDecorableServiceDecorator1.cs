namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class ConditionalDecorableServiceDecorator1 : IConditionalDecorableServiceDecorator,
                                                           IConditionalDecorator<IConditionalDecorableService, TestConditionAttribute1>
    {
        public IConditionalDecorableService Decoratee { get; }

        public ConditionalDecorableServiceDecorator1(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }
    }
}