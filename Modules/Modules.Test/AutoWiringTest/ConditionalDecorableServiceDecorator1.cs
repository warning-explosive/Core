namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(ConditionalDecorableServiceDecorator2))]
    internal class ConditionalDecorableServiceDecorator1 : IConditionalDecorableService,
                                                           IConditionalDecorableServiceDecorator<TestCondition1Attribute>
    {
        public ConditionalDecorableServiceDecorator1(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public IConditionalDecorableService Decoratee { get; }
    }
}