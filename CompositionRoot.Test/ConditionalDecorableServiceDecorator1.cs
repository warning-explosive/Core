namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class ConditionalDecorableServiceDecorator1 : IConditionalDecorableServiceDecorator,
                                                           IConditionalDecorator<IConditionalDecorableService, TestCondition1Attribute>
    {
        public ConditionalDecorableServiceDecorator1(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public IConditionalDecorableService Decoratee { get; }
    }
}