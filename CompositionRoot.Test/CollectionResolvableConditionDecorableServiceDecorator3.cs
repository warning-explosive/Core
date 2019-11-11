namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Extensions.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    public class CollectionResolvableConditionDecorableServiceDecorator3 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                           ICollectionDecorator<ICollectionResolvableConditionDecorableService>
    {
        public ICollectionResolvableConditionDecorableService Decoratee { get; }

        public CollectionResolvableConditionDecorableServiceDecorator3(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }
    }
}