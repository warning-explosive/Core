namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
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