namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableConditionDecorableServiceDecorator1), typeof(CollectionResolvableConditionDecorableServiceDecorator2))]
    internal class CollectionResolvableConditionDecorableServiceDecorator3 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                             ICollectionDecorator<ICollectionResolvableConditionDecorableService>
    {
        public CollectionResolvableConditionDecorableServiceDecorator3(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}