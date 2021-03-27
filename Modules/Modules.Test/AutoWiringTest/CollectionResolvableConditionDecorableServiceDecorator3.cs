namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableConditionDecorableServiceDecorator1), typeof(CollectionResolvableConditionDecorableServiceDecorator2))]
    internal class CollectionResolvableConditionDecorableServiceDecorator3 : ICollectionResolvableConditionDecorableService,
                                                                             ICollectionDecorator<ICollectionResolvableConditionDecorableService>
    {
        public CollectionResolvableConditionDecorableServiceDecorator3(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}