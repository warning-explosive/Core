namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(ConditionalDecorableServiceDecorator3))]
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