namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    internal class ConditionalDecorableServiceDecorator2 : IConditionalDecorableServiceDecorator,
                                                           IConditionalDecorator<IConditionalDecorableService, TestCondition2Attribute>
    {
        public ConditionalDecorableServiceDecorator2(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public IConditionalDecorableService Decoratee { get; }
    }
}