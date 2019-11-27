namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class ConditionalDecorableServiceDecorator3 : IConditionalDecorableServiceDecorator,
                                                           IDecorator<IConditionalDecorableService>
    {
        public IConditionalDecorableService Decoratee { get; }
 
        public ConditionalDecorableServiceDecorator3(IConditionalDecorableService decoratee)
        {
            Decoratee = decoratee;
        }
    }
}