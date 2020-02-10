namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

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