namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
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