namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(ConditionalDecorableServiceDecorator3))]
    internal class ConditionalDecorableServiceDecorator2 : IConditionalDecorableService,
                                                           IConditionalDecorableServiceDecorator<TestCondition2Attribute>
    {
        public ConditionalDecorableServiceDecorator2(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public IConditionalDecorableService Decoratee { get; }
    }
}